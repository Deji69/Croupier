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

struct RouletteSpinKill
{
	std::string targetName;
	bool validMethod = true;
	bool validDisguise = true;

	RouletteSpinKill(std::string targetName) : targetName(targetName)
	{ }
};

struct SharedRouletteSpin
{
	const RouletteSpin& spin;
	std::vector<RouletteSpinKill> kills;
	std::chrono::steady_clock::time_point timeStarted;
	std::chrono::seconds timeElapsed = std::chrono::seconds(0);
	bool isPlaying = false;
	bool isFinished = false;
	std::shared_mutex mutex;

	SharedRouletteSpin(const RouletteSpin& spin) : spin(spin), timeElapsed(0)
	{
		timeStarted = std::chrono::steady_clock().now();
	}

	auto getTimeElapsed() -> std::chrono::seconds {
		if (!this->isFinished && this->isPlaying) {
			return std::chrono::duration_cast<std::chrono::seconds>(std::chrono::steady_clock().now() - timeStarted);
		}
		return this->isFinished ? this->timeElapsed : std::chrono::seconds::zero();
	}

	auto playerSelectMission() {
		auto guard = std::unique_lock(this->mutex);
		this->isPlaying = false;
	}

	auto playerStart() {
		auto guard = std::unique_lock(this->mutex);
		this->kills.clear();
		if (!this->isPlaying)
			this->timeStarted = std::chrono::steady_clock().now();
		this->isPlaying = true;
		this->isFinished = false;
	}

	auto playerExit() {
		auto guard = std::unique_lock(this->mutex);
		this->timeElapsed = this->getTimeElapsed();
		this->isPlaying = false;
		this->isFinished = true;
	}
};

struct CroupierDrawInfo
{
	struct Condition
	{
		std::wstring condText;
		std::wstring killMethodText;
		std::wstring disguiseText;
		std::filesystem::path targetImage;
		std::filesystem::path killMethodImage;
		std::filesystem::path disguiseImage;
	};

	CroupierDrawInfo() = default;
	CroupierDrawInfo(const SharedRouletteSpin& spin);

	auto getTimeElapsed() -> std::chrono::seconds {
		if (!this->isFinished && this->isPlaying) {
			return std::chrono::duration_cast<std::chrono::seconds>(std::chrono::steady_clock().now() - timeStarted);
		}
		return this->isFinished ? this->timeElapsed : std::chrono::seconds::zero();
	}

	std::chrono::steady_clock::time_point timeStarted;
	std::chrono::seconds timeElapsed = std::chrono::seconds(0);
	bool isInitialised = false;
	bool isPlaying = false;
	bool isFinished = false;
	std::vector<Condition> conds;
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
	auto setTimerEnabled(bool enable) -> void;
	auto setTextMode(bool enable) -> void;

	auto paint(HWND wnd) -> void;

protected:
	auto getWindowWidth(int numConds) const -> int;
	auto getWindowHeight(int numConds) const -> int;
	auto createDeviceResources(HWND wnd) -> HRESULT;
	auto loadImage(std::filesystem::path) -> ID2D1Bitmap*;
	auto OnPaint(HWND wnd) -> LRESULT;
	auto WndProc(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam) -> LRESULT;
	static auto registerWindowClass(HINSTANCE instance, HWND parentWindow) -> ATOM;
	static auto CALLBACK s_WndProc(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam) -> LRESULT;

private:
	SharedRouletteSpin& spin;
	std::unordered_map<std::string, LoadedImage> loadedImages;
	CroupierDrawInfo latestDrawInfo;
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
	bool timer = false;
	eCroupierWindowLayout layout = eCroupierWindowLayout::ADAPTIVE;

	std::thread windowThread;
	std::filesystem::path modulePath;
	ID2D1Factory* D2DFactory = nullptr;
	ID2D1HwndRenderTarget* RT = nullptr;
	IWICImagingFactory* IWICFactory = nullptr;
	IDWriteFactory* DWriteFactory = nullptr;
	IDWriteTextFormat* DWriteTextFormat = nullptr;
	IDWriteTextFormat* DWriteTextFormatTimer = nullptr;
	ID2D1SolidColorBrush* Brush = nullptr;

	volatile bool runningWindow = false;
};
