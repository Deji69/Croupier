#pragma once
#include <exception>
#include <format>
#include <string>
#include <string_view>

class RouletteGeneratorException : public std::exception {
public:
	RouletteGeneratorException(std::string_view msg) : msg(std::format("Roulette Generator Error - {}", msg))
	{}

	auto what() const -> char const* override {
		return msg.c_str();
	}

private:
	std::string msg;
};
