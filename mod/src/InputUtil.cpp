#include <algorithm>
#include <array>
#include <string_view>
#include <Windows.h>
#include <IPluginInterface.h>

#include "InputUtil.h"

struct Key {
	constexpr Key(std::string_view name, int code) : name { name }, code { code } {}

	std::string_view name;
	int code;
};

// indices must match KeyBind::KeyCode enum
static constexpr auto keyMap = std::to_array<Key>({
	{ "NONE", 0 },
	{ "MOUSE1", 1 },
	{ "MOUSE2", 2 },
	{ "MOUSE3", 3 },
	{ "MOUSE4", 4 },
	{ "MOUSE5", 5 },
	{ "BACKSPACE", VK_BACK },
	{ "TAB", VK_TAB },
	{ "ENTER", VK_RETURN },
	{ "CAPSLOCK", VK_CAPITAL },
	{ "SPACE", VK_SPACE },
	{ "PAGE_DOWN", VK_NEXT },
	{ "PAGE_UP", VK_PRIOR },
	{ "END", VK_END },
	{ "HOME", VK_HOME },
	{ "LEFT", VK_LEFT },
	{ "UP", VK_UP },
	{ "RIGHT", VK_RIGHT },
	{ "DOWN", VK_DOWN },
	{ "INSERT", VK_INSERT },
	{ "DELETE", VK_DELETE },
	{ "0", '0' },
	{ "1", '1' },
	{ "2", '2' },
	{ "3", '3' },
	{ "4", '4' },
	{ "5", '5' },
	{ "6", '6' },
	{ "7", '7' },
	{ "8", '8' },
	{ "9", '9' },
	{ "A", 'A' },
	{ "B", 'B' },
	{ "C", 'C' },
	{ "D", 'D' },
	{ "E", 'E' },
	{ "F", 'F' },
	{ "G", 'G' },
	{ "H", 'H' },
	{ "I", 'I' },
	{ "J", 'J' },
	{ "K", 'K' },
	{ "L", 'L' },
	{ "M", 'M' },
	{ "N", 'N' },
	{ "O", 'O' },
	{ "P", 'P' },
	{ "Q", 'Q' },
	{ "R", 'R' },
	{ "S", 'S' },
	{ "T", 'T' },
	{ "U", 'U' },
	{ "V", 'V' },
	{ "W", 'W' },
	{ "X", 'X' },
	{ "Y", 'Y' },
	{ "Z", 'Z' },
	{ "NUMPAD_0", VK_NUMPAD0 },
	{ "NUMPAD_1", VK_NUMPAD1 },
	{ "NUMPAD_2", VK_NUMPAD2 },
	{ "NUMPAD_3", VK_NUMPAD3 },
	{ "NUMPAD_4", VK_NUMPAD4 },
	{ "NUMPAD_5", VK_NUMPAD5 },
	{ "NUMPAD_6", VK_NUMPAD6 },
	{ "NUMPAD_7", VK_NUMPAD7 },
	{ "NUMPAD_8", VK_NUMPAD8 },
	{ "NUMPAD_9", VK_NUMPAD9 },
	{ "MULTIPLY", VK_MULTIPLY },
	{ "ADD", VK_ADD },
	{ "SUBTRACT", VK_SUBTRACT },
	{ "DECIMAL", VK_DECIMAL },
	{ "DIVIDE", VK_DIVIDE },
	{ "F1", VK_F1 },
	{ "F2", VK_F2 },
	{ "F3", VK_F3 },
	{ "F4", VK_F4 },
	{ "F5", VK_F5 },
	{ "F6", VK_F6 },
	{ "F7", VK_F7 },
	{ "F8", VK_F8 },
	{ "F9", VK_F9 },
	{ "F10", VK_F10 },
	{ "F11", VK_F11 },
	{ "F12", VK_F12 },
	{ "LSHIFT", VK_LSHIFT },
	{ "RSHIFT", VK_RSHIFT },
	{ "LCTRL", VK_LCONTROL },
	{ "RCTRL", VK_RCONTROL },
	{ "LALT", VK_LMENU },
	{ "RALT", VK_RMENU },
	{ ";", VK_OEM_1 },
	{ "=", VK_OEM_PLUS },
	{ ",", VK_OEM_COMMA },
	{ "-", VK_OEM_MINUS },
	{ ".", VK_OEM_PERIOD },
	{ "/", VK_OEM_2 },
	{ "`", VK_OEM_3 },
	{ "[", VK_OEM_4 },
	{ "\\", VK_OEM_5 },
	{ "]", VK_OEM_6 },
	{ "'", VK_OEM_7 },
});

static bool TestModifierKey(int keyCode) {
	switch (keyCode) {
	case VK_LSHIFT:
		return GetKeyState(VK_LSHIFT) & 0x8000;
	case VK_LCONTROL:
		return GetKeyState(VK_LCONTROL) & 0x8000;
	case VK_LMENU:
		return GetKeyState(VK_LMENU) & 0x8000;
	case VK_RSHIFT:
		return GetKeyState(VK_RSHIFT) & 0x8000;
	case VK_RCONTROL:
		return GetKeyState(VK_RCONTROL) & 0x8000;
	case VK_RMENU:
		return GetKeyState(VK_RMENU) & 0x8000;
	}
	return false;
}

KeyBind::KeyBind(KeyCode keyCode) noexcept : keyCode { static_cast<std::size_t>(keyCode) < keyMap.size() ? keyCode : -1 } {}

KeyBind::KeyBind(const char* keyName) noexcept
{
	const auto it = find_if(begin(keyMap), end(keyMap), [keyName](const Key& key) { return key.name == keyName; });
	if (it != end(keyMap))
		keyCode = it->code;
	else
		keyCode = -1;
}

const char* KeyBind::toString() const noexcept
{
	auto it = std::ranges::find(keyMap, keyCode, &Key::code);
	return it != end(keyMap) ? it->name.data() : "";
}

const int KeyBind::toInt() const noexcept
{
	return keyCode;
}

bool KeyBind::isPressed() const noexcept
{
	if (keyCode == -1)
		return false;

	if (keyCode >= 1 && keyCode <= 5)
		return ImGui::IsMouseClicked(keyCode - 1);

	return ImGui::IsKeyPressed(keyCode, false);
}

bool KeyBind::isDown() const noexcept
{
	if (keyCode == -1)
		return false;

	if (keyCode >= 1 && keyCode <= 5)
		return ImGui::IsMouseDown(keyMap[keyCode].code);

	if (TestModifierKey(keyCode))
		return true;

	return ImGui::IsKeyDown(keyCode);
}

bool KeyBind::setToPressedKey() noexcept
{
	if (ImGui::IsKeyPressed(ImGui::GetIO().KeyMap[ImGuiKey_Escape])) {
		keyCode = -1;
		return true;
	}
	else if (TestModifierKey(VK_LSHIFT)) {
		keyCode = VK_LSHIFT;
		return true;
	}
	else if (TestModifierKey(VK_LMENU)) {
		keyCode = VK_LMENU;
		return true;
	}
	else if (TestModifierKey(VK_LCONTROL)) {
		keyCode = VK_LCONTROL;
		return true;
	}

	for (int i = 0; i < IM_ARRAYSIZE(ImGui::GetIO().MouseDown); ++i) {
		if (ImGui::IsMouseClicked(i)) {
			keyCode = KeyCode(1 + i);
			return true;
		}
	}

	for (int i = 0; i < IM_ARRAYSIZE(ImGui::GetIO().KeysDown); ++i) {
		if (!ImGui::IsKeyPressed(i))
			continue;

		if (const auto it = std::ranges::find(keyMap, i, &Key::code); it != keyMap.end()) {
			keyCode = it->code;
			// Treat AltGr as RALT
			if (keyCode == VK_LCONTROL && ImGui::IsKeyPressed(VK_RMENU))
				keyCode = VK_RMENU;
			return true;
		}
	}
	return false;
}

void KeyBindToggle::handleToggle() noexcept
{
	if (isPressed())
		toggledOn = !toggledOn;
}
