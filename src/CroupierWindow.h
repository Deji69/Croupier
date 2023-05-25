#pragma once
#include <filesystem>
#include <mutex>
#include <shared_mutex>
#include <string>
#include <thread>
#include <Windows.h>
#include <d2d1.h>
#include <dwrite.h>
#include <wincodec.h>

#include "Roulette.h"

#define CROUPIER_UPDATE_WINDOW (WM_USER + 0x02)
#define CROUPIER_CLOSE_WINDOW (WM_USER + 0x8009)

enum class eCroupierWindowLayout {
	SINGLE,
	ADAPTIVE,
	WIDE,
};

struct LoadedImage {
	ID2D1Bitmap* D2DBitmap = nullptr;
	IWICFormatConverter* ConvertedSourceBitmap = nullptr;

	LoadedImage() = default;
	LoadedImage(LoadedImage&&) noexcept;
	LoadedImage(const LoadedImage&) = delete;
	~LoadedImage();

	auto getBitmap(ID2D1HwndRenderTarget* rt) {
		// D2DBitmap may have been released due to device loss. 
		// If so, re-create it from the source bitmap
		if (this->ConvertedSourceBitmap && !this->D2DBitmap) {
			rt->CreateBitmapFromWicBitmap(this->ConvertedSourceBitmap, NULL, &this->D2DBitmap);
		}
		return this->D2DBitmap;
	}
};

struct SharedRouletteSpin
{
	const RouletteSpin& spin;
	std::shared_mutex mutex;

	SharedRouletteSpin(const RouletteSpin& spin) : spin(spin)
	{ }
};

class CroupierWindow
{
public:
	CroupierWindow(SharedRouletteSpin&);
	~CroupierWindow();

	auto create() -> HRESULT;
	auto update() -> void;
	auto destroy() -> void;

	auto setDarkMode(bool enable) -> void;
	auto setAlwaysOnTop(bool enable) -> void;
	auto setTextMode(bool enable) -> void;

	auto paint(HWND wnd) -> void;

protected:
	auto getWindowWidth() const -> int;
	auto getWindowHeight() const -> int;
	auto createDeviceResources(HWND wnd) -> HRESULT;
	auto loadImage(std::filesystem::path) -> ID2D1Bitmap*;
	auto OnPaint(HWND wnd) -> LRESULT;
	auto WndProc(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam) -> LRESULT;
	static auto registerWindowClass(HINSTANCE instance, HWND parentWindow) -> ATOM;
	static auto CALLBACK s_WndProc(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam) -> LRESULT;

private:
	SharedRouletteSpin& spin;
	std::unordered_map<std::string, LoadedImage> loadedImages;
	HINSTANCE hInst = nullptr;
	HWND hWnd = nullptr;
	HRESULT initialization = S_OK;
	HWND column1 = nullptr;
	HWND column2 = nullptr;
	ATOM wclAtom = NULL;
	bool darkMode = true;
	bool onTop = true;
	bool wasOnTop = false;
	bool alignRight = false;
	bool textMode = false;
	eCroupierWindowLayout layout = eCroupierWindowLayout::ADAPTIVE;

	std::thread windowThread;
	std::filesystem::path modulePath;
	ID2D1Factory* D2DFactory = nullptr;
	ID2D1HwndRenderTarget* RT = nullptr;
	IWICImagingFactory* IWICFactory = nullptr;
	IDWriteFactory* DWriteFactory = nullptr;
	IDWriteTextFormat* DWriteTextFormat = nullptr;
	ID2D1SolidColorBrush* Brush = nullptr;

	volatile bool runningWindow = false;
};
