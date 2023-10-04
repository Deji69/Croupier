#include <Windows.h>
#include <wincodec.h>
#include <commdlg.h>
#include <d2d1.h>
#include <d2d1helper.h>
#include <dwrite.h>

#include <array>
#include <filesystem>
#include <format>
#include <mutex>
#include <string>
#include <thread>
#include <tuple>
#include <vector>

#include "CroupierWindow.h"
#include "Logging.h"
#include "FixMinMax.h"
#include "util.h"

#define IDT_TIMER1 1001

using namespace std::string_literals;

HINSTANCE hInstance = nullptr;
const FLOAT DEFAULT_DPI = 96.f;
const FLOAT TEXT_ROW_HEIGHT = 40.0f;

template<typename T>
inline auto SafeRelease(T*& p) {
	if (!p) return;
	p->Release();
	p = nullptr;
}

LoadedImage::LoadedImage(LoadedImage&& other) noexcept :
	ConvertedSourceBitmap(other.ConvertedSourceBitmap),
	D2DBitmap(other.D2DBitmap)
{
	other.ConvertedSourceBitmap = nullptr;
	other.D2DBitmap = nullptr;
}

LoadedImage::~LoadedImage() {
	SafeRelease(this->D2DBitmap);
	SafeRelease(this->ConvertedSourceBitmap);
}

auto APIENTRY DllMain(HMODULE module, DWORD reason, LPVOID) -> BOOL {
	HeapSetInformation(NULL, HeapEnableTerminationOnCorruption, NULL, 0);

	switch (reason) {
		case DLL_PROCESS_ATTACH:
		case DLL_THREAD_ATTACH:
			hInstance = module;
			break;
		case DLL_THREAD_DETACH:
		case DLL_PROCESS_DETACH:
			break;
	}
	return TRUE;
}

CroupierWindow::CroupierWindow(SharedRouletteSpin& spin) : spin(spin)
{
	this->initialization = CoInitializeEx(NULL, COINIT_APARTMENTTHREADED | COINIT_DISABLE_OLE1DDE);
}

CroupierWindow::~CroupierWindow()
{
	this->destroy();
	if (this->wclAtom) UnregisterClass((LPCSTR)this->wclAtom, NULL);
}

auto CroupierWindow::getWindowWidth(int numConds) const -> int
{
	if (this->textMode || numConds < 2) return 640;
	auto const wide = this->layout == eCroupierWindowLayout::WIDE || (this->layout == eCroupierWindowLayout::ADAPTIVE && numConds > 2);
	return wide ? 940 : 640;
}

auto CroupierWindow::getWindowHeight(int numConds) const -> int
{
	if (!numConds && !this->textMode) return 420;
	auto const wide = !this->textMode && (this->layout == eCroupierWindowLayout::WIDE || (this->layout == eCroupierWindowLayout::ADAPTIVE && numConds > 2));
	auto const rows = wide ? (numConds + 1) / 2 : numConds;
	if (this->textMode) return rows * TEXT_ROW_HEIGHT + 45 + (this->timer ? 30 : 0);
	if (!wide) return rows * 195.0f + 45;
	switch (numConds) {
	case 1: return 250;
	case 2: return 185;
	case 3:
	case 4: return 336;
	case 5: return 475;
	}
	return 420;
}

auto CroupierWindow::registerWindowClass(HINSTANCE instance, HWND parentWindow) -> ATOM
{
	auto wcl = WNDCLASSEXA{};
	wcl.cbSize = sizeof(WNDCLASSEXA);
	wcl.style = CS_OWNDC;
	wcl.lpfnWndProc = &CroupierWindow::s_WndProc;
	wcl.cbWndExtra = sizeof(LONG_PTR);
	wcl.hInstance = instance;
	wcl.hIcon = reinterpret_cast<HICON>(GetClassLongPtr(parentWindow, GCLP_HICON));
	wcl.hCursor = LoadCursor(NULL, IDC_ARROW);
	wcl.hbrBackground = NULL;
	wcl.lpszMenuName = NULL;
	wcl.lpszClassName = "Croupier";
	wcl.hIconSm = reinterpret_cast<HICON>(GetClassLongPtr(parentWindow, GCLP_HICONSM));
	return RegisterClassEx(&wcl);
}

auto CroupierWindow::create() -> HRESULT
{
	if (!SUCCEEDED(this->initialization)) return 0;

	if (SUCCEEDED(this->initialization))
		this->initialization = CoCreateInstance(CLSID_WICImagingFactory, NULL, CLSCTX_INPROC_SERVER, IID_PPV_ARGS(&this->IWICFactory));

	auto hitmanWindow = FindWindow(NULL, "HITMAN 3");
	if (!hitmanWindow) {
		Logger::Warn("Croupier: could not locate game window.");
		return E_FAIL;
	}

	if (modulePath.empty()) {
		CHAR filename[MAX_PATH] = {};
		if (GetModuleFileName(hInstance, filename, MAX_PATH) != 0)
			this->modulePath = std::filesystem::path(filename).parent_path();
	}

	if (SUCCEEDED(this->initialization)) {
		this->initialization = S_OK;

		windowThread = std::thread([this, hitmanWindow] {
			if (!this->wclAtom) this->wclAtom = this->registerWindowClass(hInstance, hitmanWindow);

			auto width = 0, height = 0;

			{
				auto guard = std::shared_lock(this->spin.mutex);
				auto const numConds = this->spin.spin.getConditions().size();
				width = this->getWindowWidth(numConds);
				height = this->getWindowHeight(numConds);
			}

			this->hWnd = CreateWindow(
				"Croupier",
				"Croupier",
				WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_MINIMIZEBOX/* | WS_THICKFRAME*/,	// TODO: figure out resizing contents
				CW_USEDEFAULT, CW_USEDEFAULT,
				width, height,
				NULL, NULL,
				hInstance,
				this
			);

			if (!this->hWnd) {
				Logger::Error("Croupier: failed to create external window - {}", GetLastError());
				this->runningWindow = false;
				return;
			}

			ShowWindow(this->hWnd, SW_SHOW);
			UpdateWindow(this->hWnd);
			PostMessage(this->hWnd, CROUPIER_UPDATE_WINDOW, 0, 0);

			SetTimer(this->hWnd, IDT_TIMER1, 50, (TIMERPROC)NULL);

			auto msg = MSG{};
			auto ret = BOOL{};

			while ((ret = GetMessage(&msg, NULL, 0, 0)) != 0) {
				if (ret == -1) break;
				else {
					TranslateMessage(&msg);
					DispatchMessage(&msg);

					if (msg.message == CROUPIER_CLOSE_WINDOW) {
						if (this->hWnd) {
							DestroyWindow(this->hWnd);
							this->hWnd = nullptr;
						}
						break;
					}
					else if (this->hWnd) {
						if (msg.message == WM_TIMER) {
							auto elapsed = std::chrono::seconds(0);
							{
								auto guard = std::shared_lock(this->spin.mutex);
								elapsed = this->spin.getTimeElapsed();
							}
							if (msg.wParam == IDT_TIMER1 && this->timer && elapsed > this->latestDrawInfo.timeElapsed)
								InvalidateRect(this->hWnd, NULL, true);
						}
						else if (msg.message == CROUPIER_UPDATE_WINDOW) {
							auto numConds = 0;
							{
								auto guard = std::shared_lock(this->spin.mutex);
								numConds = this->spin.spin.getConditions().size();
							}
							auto const w = this->getWindowWidth(numConds);
							auto const h = this->getWindowHeight(numConds);
							InvalidateRect(this->hWnd, NULL, true);

							if (this->wasOnTop != this->onTop)
								SetWindowPos(this->hWnd, this->onTop ? HWND_TOPMOST : HWND_BOTTOM, 0, 0, w, h, SWP_NOMOVE);
							else
								SetWindowPos(this->hWnd, HWND_TOPMOST, 0, 0, w, h, SWP_NOZORDER | SWP_NOMOVE);
						}
					}
				}
			}

			this->runningWindow = false;
		});
	}

	return this->initialization;
}

auto getCellPosY(int row)
{
	constexpr int marginTop = 5;
	return marginTop + (60 * row);
}

auto CroupierWindow::paint(HWND wnd) -> void
{
	auto ps = PAINTSTRUCT{};
	auto rect = RECT{};

	GetClientRect(wnd, &rect);

	auto hdc = BeginPaint(wnd, &ps);
	auto oldBkMode = SetBkMode(hdc, TRANSPARENT);

	SetBkMode(hdc, oldBkMode);
	EndPaint(wnd, &ps);
}

auto CroupierWindow::update() -> void
{
	PostThreadMessage(GetThreadId(this->windowThread.native_handle()), CROUPIER_UPDATE_WINDOW, 0, 0);
}

auto CroupierWindow::destroy() -> void
{
	if (this->hWnd) {
		PostThreadMessage(GetThreadId(this->windowThread.native_handle()), CROUPIER_CLOSE_WINDOW, 0, 0);
		this->windowThread.detach();
	}

	this->loadedImages.clear();

	SafeRelease(this->Brush);
	SafeRelease(this->DWriteTextFormat);
	SafeRelease(this->DWriteFactory);
	SafeRelease(this->IWICFactory);
	SafeRelease(this->D2DFactory);
	SafeRelease(this->RT);
}

auto CroupierWindow::loadImage(std::filesystem::path path) -> ID2D1Bitmap*
{
	auto hr = S_OK;

	IWICBitmapDecoder* decoder = nullptr;
	IWICBitmapFrameDecode* frame = nullptr;

	auto it = this->loadedImages.find(path.string());
	if (it != this->loadedImages.end())
		return it->second.getBitmap(this->RT);

	auto image = LoadedImage();
	auto fullPath = this->modulePath / "Croupier" / path;

	if (!std::filesystem::is_regular_file(fullPath))
		hr = E_FAIL;

	if (SUCCEEDED(hr))
		hr = this->IWICFactory->CreateDecoderFromFilename(fullPath.c_str(), nullptr, GENERIC_READ, WICDecodeMetadataCacheOnDemand, &decoder);

	if (SUCCEEDED(hr))
		hr = decoder->GetFrame(0, &frame);

	if (SUCCEEDED(hr))
		hr = this->IWICFactory->CreateFormatConverter(&image.ConvertedSourceBitmap);

	if (SUCCEEDED(hr))
		hr = image.ConvertedSourceBitmap->Initialize(
			frame,
			GUID_WICPixelFormat32bppPBGRA,
			WICBitmapDitherTypeNone,
			NULL,
			0.f,
			WICBitmapPaletteTypeCustom
		);

	if (SUCCEEDED(hr))
		hr = this->createDeviceResources(this->hWnd);

	if (SUCCEEDED(hr))
		hr = this->RT->CreateBitmapFromWicBitmap(image.ConvertedSourceBitmap, NULL, &image.D2DBitmap);

	SafeRelease(decoder);
	SafeRelease(frame);

	if (FAILED(hr))
		return nullptr;

	auto pr = this->loadedImages.emplace(path.string(), std::move(image));
	return pr.first->second.getBitmap(this->RT);
}

CroupierDrawInfo::CroupierDrawInfo(const SharedRouletteSpin& spin) {
	this->timeStarted = spin.timeStarted;
	this->isFinished = spin.isFinished;
	this->isPlaying = spin.isPlaying;
	this->isInitialised = true;
	for (const auto& cond : spin.spin.getConditions()) {
		Condition condition;
		condition.condText = widen(std::format("{}: {} / {}", cond.target.get().getName(), cond.methodName, cond.disguise.get().name));
		condition.killMethodText = widen(cond.methodName);
		condition.disguiseText = widen(cond.disguise.get().name);
		condition.killMethodImage = std::filesystem::path(cond.killMethod.method == eKillMethod::NONE ? "weapons"s : ""s) / cond.killMethod.image;
		condition.targetImage = std::filesystem::path("actors"s) / cond.target.get().getImage();
		condition.disguiseImage = std::filesystem::path("outfits"s) / cond.disguise.get().image;
		this->conds.push_back(std::move(condition));
	}
}

auto CroupierWindow::OnPaint(HWND wnd) -> LRESULT
{
	HRESULT hr = S_OK;
	PAINTSTRUCT ps = {};

	if (this->spin.mutex.try_lock_shared()) {
		this->latestDrawInfo = CroupierDrawInfo(this->spin);
		this->spin.mutex.unlock_shared();
	}

	if (BeginPaint(wnd, &ps)) {
		hr = this->createDeviceResources(wnd);

		if (SUCCEEDED(hr) && !(this->RT->CheckWindowState() & D2D1_WINDOW_STATE_OCCLUDED)) {
			this->RT->BeginDraw();
			this->RT->SetTransform(D2D1::Matrix3x2F::Identity());
			this->RT->Clear(D2D1::ColorF(D2D1::ColorF::Black));

			if (this->latestDrawInfo.isInitialised) {
				auto rtSize = this->RT->GetPixelSize();

				auto const numConds = this->latestDrawInfo.conds.size();
				auto width = this->getWindowWidth(numConds);
				auto height = this->getWindowHeight(numConds);
				auto const& conds = this->latestDrawInfo.conds;
				auto const wide = (this->layout == eCroupierWindowLayout::WIDE || (this->layout == eCroupierWindowLayout::ADAPTIVE && numConds > 2)) && numConds >= 2;
				auto scale = rtSize.width / (wide && !this->textMode ? 1280.0f : 640.0f);
				auto scaler = rtSize.width / 640.0f;
				auto getRect = [scale, rtSize](FLOAT x, FLOAT y, FLOAT w, FLOAT h){ // grb
					if (x < 0) x = rtSize.width + x;
					if (y < 0) y = rtSize.height + y;
					if (w < 0) w = rtSize.width + w;
					if (h < 0) h = rtSize.height + h;
					return D2D1::RectF(x * scale, y * scale, (x + w) * scale, (y + h) * scale);
				};
				auto getRect2 = [scaler, rtSize](FLOAT x, FLOAT y, FLOAT w, FLOAT h) {
					if (w < 0) w = 640 + w;
					if (h < 0) h = rtSize.height + h;
					if (x < 0) x = 640 - w + x;
					if (y < 0) y = rtSize.height - h + y;
					w += x;
					h += y;
					//x *= scaler;
					//w *= scaler;
					return D2D1::RectF(x * scaler, y, w * scaler, h);
				};
				auto n = 0;

				for (auto const& cond : this->latestDrawInfo.conds) {
					auto const row = !this->textMode && wide ? n / 2 : n;

					if (this->textMode) {
						auto const top = TEXT_ROW_HEIGHT * row;
						auto textRect = getRect(6, top + 5, 640 - 6, TEXT_ROW_HEIGHT);
						this->RT->DrawTextA(
							cond.condText.c_str(),
							cond.condText.size(),
							this->DWriteTextFormat,
							textRect,
							this->Brush
						);
					} else {
						auto const col = wide ? n % 2 : 0;
						auto const top = 200 * row;
						auto const left = 640 * col + (wide && n == numConds - 1 && numConds % 2 == 1 ? 640 : 0);

						auto targetImage = this->loadImage(cond.targetImage);
						auto killMethodImage = this->loadImage(cond.killMethodImage);
						auto disguiseImage = this->loadImage(cond.disguiseImage);

						auto targetImageRect = getRect(left + 0, top, 260, 200);
						auto methodImageRect = getRect(left + 260, top, 134, 100);
						auto disguiseImageRect = getRect(left + 260, top + 100, 134, 100);

						if (targetImage) this->RT->DrawBitmap(targetImage, targetImageRect);
						if (killMethodImage) this->RT->DrawBitmap(killMethodImage, methodImageRect);
						if (disguiseImage) this->RT->DrawBitmap(disguiseImage, disguiseImageRect);

						auto const imagesWidth = this->textMode ? 0 : 260 + 134;
						auto methodTextRect = getRect(left + imagesWidth + 6, top + 5, 220, 90);
						auto disguiseTextRect = getRect(left + imagesWidth + 6, top + 100 + 5, 220, 90);

						this->RT->DrawTextA(
							cond.killMethodText.c_str(),
							cond.killMethodText.size(),
							this->DWriteTextFormat,
							methodTextRect,
							this->Brush
						);
						this->RT->DrawTextA(
							cond.disguiseText.c_str(),
							cond.disguiseText.size(),
							this->DWriteTextFormat,
							disguiseTextRect,
							this->Brush
						);
					}

					++n;

					if (this->timer) {
						auto const elapsed = this->latestDrawInfo.getTimeElapsed();
						auto timeFormat = std::string();
						auto const includeHr = std::chrono::duration_cast<std::chrono::hours>(elapsed).count() >= 1;
						auto const left = wide && conds.size() % 2;
						auto const time = widen(includeHr ? std::format("{:%H:%M:%S}", elapsed) : std::format("{:%M:%S}", elapsed));

						if (this->textMode) {
							this->RT->DrawTextA(
								time.c_str(),
								time.size(),
								this->DWriteTextFormat,
								getRect2(6, -5.0, 640.0 - 6, 22.0),
								this->Brush
							);
						}
						else {
							this->RT->DrawTextA(
								time.c_str(),
								time.size(),
								this->DWriteTextFormat,
								getRect2(left ? 6.0 : -5.0, -5.0, includeHr ? 80 : 55, 22.0),
								this->Brush
							);
						}
					}
				}
			}

			hr = this->RT->EndDraw();
			
			if (hr == D2DERR_RECREATE_TARGET) {
				for (auto& image : this->loadedImages) {
					SafeRelease(image.second.D2DBitmap);
				}
				SafeRelease(this->RT);
				hr = InvalidateRect(hWnd, NULL, TRUE) ? S_OK : E_FAIL;
			}
			else if (!SUCCEEDED(hr)) {
				Logger::Error("D2D Err {}", hr);
			}
		}

		EndPaint(wnd, &ps);
	}

	return SUCCEEDED(hr) ? 0 : 1;
}

auto CroupierWindow::WndProc(HWND wnd, UINT msg, WPARAM wParam, LPARAM lParam) -> LRESULT
{
	switch (msg) {
		case WM_SIZE: {
			D2D1_SIZE_U size = D2D1::SizeU(LOWORD(lParam), HIWORD(lParam));
			if (this->RT) {
				if (FAILED(this->RT->Resize(size))) {
					SafeRelease(this->RT);
					for (auto& image : this->loadedImages) {
						SafeRelease(image.second.D2DBitmap);
					}
				}
			}
			return 0;
		}
		case WM_PAINT:
			return this->OnPaint(wnd);
		case WM_CREATE:
			this->update();
			break;
	}
	return DefWindowProc(wnd, msg, wParam, lParam);
}

auto CALLBACK CroupierWindow::s_WndProc(HWND wnd, UINT msg, WPARAM wParam, LPARAM lParam) -> LRESULT {
	if (msg == WM_CREATE) {
		SetWindowLongPtr(wnd, GWLP_USERDATA, PtrToUlong(reinterpret_cast<CREATESTRUCT*>(lParam)->lpCreateParams));
	} else {
		auto window = reinterpret_cast<CroupierWindow*>(GetWindowLongPtr(wnd, GWLP_USERDATA));
		if (window) return window->WndProc(wnd, msg, wParam, lParam);
	}
 
	return DefWindowProc(wnd, msg, wParam, lParam);
}

auto CroupierWindow::createDeviceResources(HWND wnd) -> HRESULT
{
	HRESULT hr = S_OK;

	if (SUCCEEDED(hr) && !this->D2DFactory)
		hr = D2D1CreateFactory(D2D1_FACTORY_TYPE_SINGLE_THREADED, &this->D2DFactory);

	if (SUCCEEDED(hr) && !this->DWriteFactory)
		hr = DWriteCreateFactory(DWRITE_FACTORY_TYPE_SHARED, __uuidof(this->DWriteFactory), reinterpret_cast<IUnknown**>(&this->DWriteFactory));

	if (SUCCEEDED(hr) && !this->DWriteTextFormat) {
		hr = this->DWriteFactory->CreateTextFormat(
			L"Arial",
			NULL,
			DWRITE_FONT_WEIGHT_NORMAL, DWRITE_FONT_STYLE_NORMAL, DWRITE_FONT_STRETCH_NORMAL,
			21,
			L"",
			&this->DWriteTextFormat
		);

		if (SUCCEEDED(hr)) {
			this->DWriteTextFormat->SetTextAlignment(DWRITE_TEXT_ALIGNMENT_LEADING);
			this->DWriteTextFormat->SetParagraphAlignment(DWRITE_PARAGRAPH_ALIGNMENT_NEAR);
		}
	}

	if (SUCCEEDED(hr) && !this->RT) {
		RECT rc;
		hr = GetClientRect(this->hWnd, &rc) ? S_OK : E_FAIL;

		if (SUCCEEDED(hr)) {
			auto renderTargetProps = D2D1::RenderTargetProperties();
			// Set the DPI to be the default system DPI to allow direct mapping
			// between image pixels and desktop pixels in different system DPI settings
			renderTargetProps.dpiX = DEFAULT_DPI;
			renderTargetProps.dpiY = DEFAULT_DPI;

			D2D1_SIZE_U size = D2D1::SizeU(rc.right - rc.left, rc.bottom - rc.top);

			hr = this->D2DFactory->CreateHwndRenderTarget(
				renderTargetProps,
				D2D1::HwndRenderTargetProperties(this->hWnd, size),
				&this->RT
			);
		}
	}

	if (SUCCEEDED(hr) && !this->Brush) {
		hr = this->RT->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::White, 1.0f), &this->Brush);
	}

	return hr;
}

auto CroupierWindow::setDarkMode(bool enable) -> void
{
	this->darkMode = enable;
	this->update();
}

auto CroupierWindow::setAlwaysOnTop(bool enable) -> void
{
	this->onTop = enable;
	this->update();
}

auto CroupierWindow::setTimerEnabled(bool enable) -> void
{
	this->timer = enable;
	this->update();
}

auto CroupierWindow::setTextMode(bool enable) -> void
{
	this->textMode = enable;
	this->update();
}
