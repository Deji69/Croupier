#pragma once
#include <random>
#include <vector>

std::random_device randomDevice;
std::mt19937 randomGenerator;

template<typename T>
static auto randomVectorElement(const std::vector<T>& vec) -> const T&
{
	std::uniform_int_distribution<> dist(0, vec.size() - 1);
	return vec[dist(randomGenerator)];
}

template<typename T>
static auto randomVectorIndex(const std::vector<T>& vec) -> int
{
	std::uniform_int_distribution<> dist(0, vec.size() - 1);
	return dist(randomGenerator);
}

static auto randomBool() -> bool
{
	std::uniform_int_distribution<> dist(0, 1);
	return dist(randomGenerator) != 0;
}

static auto randomBool(int percentage) -> bool
{
	std::uniform_int_distribution<> dist(0, 100);
	return dist(randomGenerator) <= percentage;
}
