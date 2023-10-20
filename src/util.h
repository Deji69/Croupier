#pragma once
#include <algorithm>
#include <codecvt>
#include <locale>
#include <string>
#include <vector>

// helper type for the visitor #4
template<class... Ts>
struct overloaded : Ts... { using Ts::operator()...; };
// explicit deduction guide (not needed as of C++20)
template<class... Ts>
overloaded(Ts...) -> overloaded<Ts...>;

inline std::wstring widen(const std::string& str)
{
	using convert_typeX = std::codecvt_utf8<wchar_t>;
	std::wstring_convert<convert_typeX, wchar_t> converterX;
	return converterX.from_bytes(str);
}

inline std::string narrow(const std::wstring& wstr)
{
	using convert_typeX = std::codecvt_utf8<wchar_t>;
	std::wstring_convert<convert_typeX, wchar_t> converterX;
	return converterX.to_bytes(wstr);
}

inline auto ltrim(std::string_view str, std::string_view delimiter = " \t\n\r") {
	if (str.empty()) return str;
	auto const n = str.find_first_not_of(delimiter);
	return n == str.npos ? str.substr(0, 0) : str.substr(n);
}

inline auto rtrim(std::string_view str, std::string_view delimiter = " \t\n\r") {
	if (str.empty()) return str;
	auto const n = str.find_last_not_of(delimiter);
	return str.substr(0, n == str.npos ? 0 : n + 1);
}

inline auto trim(std::string_view str, std::string_view delimiter = " \t\n\r") -> std::string_view {
	return ltrim(rtrim(str));
}

inline auto split(std::string_view p_String, const std::string& p_Delimeter, int limit = -1) -> std::vector<std::string_view>
{
	std::vector<std::string_view> s_Parts;

	size_t s_PartStart = p_String.find_first_not_of(p_Delimeter);
	size_t s_PartEnd;

	while ((s_PartEnd = p_String.find_first_of(p_Delimeter, s_PartStart)) != std::string::npos)
	{
		if (limit != -1 && s_Parts.size() >= (limit - 1)) break;
		s_Parts.push_back(p_String.substr(s_PartStart, s_PartEnd - s_PartStart));
		s_PartStart = p_String.find_first_not_of(p_Delimeter, s_PartEnd);
	}

	if (s_PartStart != std::string::npos)
		s_Parts.push_back(p_String.substr(s_PartStart));

	return s_Parts;
}

inline auto toLowerCase(std::string_view p_String) -> std::string
{
	std::string s_String = std::string(p_String);
	std::transform(s_String.begin(), s_String.end(), s_String.begin(), ::tolower);
	return s_String;
}

inline auto toUpperCase(std::string_view p_String) -> std::string
{
	std::string s_String = std::string(p_String);
	std::transform(s_String.begin(), s_String.end(), s_String.begin(), ::toupper);
	return s_String;
}
