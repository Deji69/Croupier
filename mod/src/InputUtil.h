#pragma once
#include <string>

using std::string;

using KeyCode = int;

class KeyBind
{
public:
	KeyBind() : keybind { "" } {};
	KeyBind(string _keybind) : keybind { _keybind } {};
	string keybind;

	//KeyBind() = default;
	explicit KeyBind(KeyCode keyCode) noexcept;
	explicit KeyBind(const char* keyName) noexcept;

	bool operator==(KeyCode keyCode) const noexcept { return this->keyCode == keyCode; }
	friend bool operator==(const KeyBind& a, const KeyBind& b) noexcept { return a.keyCode == b.keyCode; }

	[[nodiscard]] const char* toString() const noexcept;
	[[nodiscard]] const int toInt() const noexcept;
	[[nodiscard]] bool isPressed() const noexcept;
	[[nodiscard]] bool isDown() const noexcept;
	[[nodiscard]] bool isSet() const noexcept { return keyCode != -1; }

	bool setToPressedKey() noexcept;

private:
	KeyCode keyCode = -1;
};

class KeyBindToggle : public KeyBind
{
public:
	using KeyBind::KeyBind;

	void handleToggle() noexcept;
	[[nodiscard]] bool isToggled() const noexcept { return toggledOn; }

private:
	bool toggledOn = true;
};
