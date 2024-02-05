#pragma once
#include <optional>
#include <string>
#include <unordered_map>
#include <unordered_set>
#include <variant>
#include <vector>
#include "Roulette.h"

namespace {
	struct KeywordInitialiser {
		static const KeywordInitialiser inst;

		KeywordInitialiser();
	};
}

struct DisguiseKeywords {
	std::unordered_set<eMission> missions;
	std::unordered_map<std::string, std::string> keywords;

	DisguiseKeywords(std::unordered_set<eMission> missions = {}, std::unordered_map<std::string, std::string> keywords = {}) : missions(missions), keywords(keywords)
	{ }
};
struct MethodKeywords {
	std::unordered_set<eMission> missions;
	std::unordered_map<std::string, std::string> keywords;

	MethodKeywords(std::unordered_set<eMission> missions = {}, std::unordered_map<std::string, std::string> keywords = {}) : missions(missions), keywords(keywords)
	{ }
};

extern std::unordered_map<std::string, std::string> targetKeywords;
extern const std::unordered_map<std::string, eKillComplication> complicationKeywords;
extern const std::unordered_map<std::string, eKillType> killTypeKeywords;
extern const std::unordered_map<std::string, std::string> keywordKeywords;
extern const std::unordered_map<std::string, std::string> methodKeywords;
extern const std::vector<DisguiseKeywords> disguiseKeywords;
inline std::wstring_convert<std::codecvt_utf8<wchar_t>> converter;

inline std::string processInput(std::string_view input) {
	auto wideInput = converter.from_bytes(input.data());
	auto classicLocale = std::locale("C");

	std::string result;
	for (wchar_t wch : wideInput) {
		auto& facet = std::use_facet<std::ctype<wchar_t>>(classicLocale);
		if (!facet.is(facet.alnum | facet.space, wch)) continue;
		if (wch == 0xFC) result += 'u';
		else result += facet.tolower(facet.narrow(wch, ' '));
	}

	return removeDiacritics(result);
}

struct ParseConditionContext {
	std::vector<std::string>& tokens;
	size_t conditions = 0;
	size_t nextIndex = 0;
	eMission mission = eMission::NONE;
	std::string target;
	std::string disguise;
	eKillMethod killMethod = eKillMethod::NONE;
	eMapKillMethod mapMethod = eMapKillMethod::NONE;
	eKillType killType = eKillType::Any;
	eKillComplication complication = eKillComplication::None;

	ParseConditionContext(std::vector<std::string>& tokens) : tokens(tokens) {};
};

class SpinParser
{
public:
	static auto createSpinFromParseContexts(const std::vector<ParseConditionContext>& contexts) -> std::optional<RouletteSpin> {
		if (contexts.empty()) return nullptr;
		auto mission = Missions::get(contexts.front().mission);
		if (!mission) return nullptr;

		RouletteSpin spin(mission);

		for (auto& target : mission->getTargets()) {
			auto it = find_if(begin(contexts), end(contexts), [&target](const ParseConditionContext& ctx) {
				return ctx.target == target.getName();
			});
			if (it == end(contexts)) return nullptr;

			auto& context = *it;
			auto disguise = mission->getDisguiseByName(context.disguise);
			if (!disguise) disguise = mission->getSuitDisguise();
			if (!disguise) return nullptr;
			auto killMethod = context.killMethod;
			if (killMethod == eKillMethod::NONE && context.mapMethod == eMapKillMethod::NONE)
				killMethod = eKillMethod::NeckSnap;
			spin.add(RouletteSpinCondition{target, *disguise, KillMethod{killMethod}, MapKillMethod{context.mapMethod}, context.killType, context.complication});
		}

		return spin;
	}

	static auto parse(std::string_view input) -> std::optional<RouletteSpin> {
		std::vector<ParseConditionContext> contexts;
		std::vector<std::string> tokens;
		contexts.push_back(ParseConditionContext{tokens});
		auto detectedMission = eMission::NONE;
		auto processed = processInput(input);

		while (parseCondition(processed, contexts.back())) {
			auto mission = contexts.back().mission;
			auto nextIndex = contexts.back().nextIndex;

			if (detectedMission == eMission::NONE)
				detectedMission = mission;
			else if (mission != detectedMission)
				return nullptr;

			contexts.push_back(ParseConditionContext{tokens});
			contexts.back().nextIndex = nextIndex;
			contexts.back().mission = mission;
			contexts.back().conditions = contexts.size();
		}

		return createSpinFromParseContexts(contexts);
	}

	static auto parseCondition(std::string_view processed, ParseConditionContext& context) -> bool {
		auto parseTargetToken = [&context](const std::string& token){
			auto it = ::targetKeywords.find(token);
			if (it != end(::targetKeywords)) {
				if (context.target.empty()) {
					context.target = Keyword::targetKeyToName(it->second);
					context.mission = getMissionForTarget(context.target);
				}
				return true;
			}
			return false;
		};

		auto parseToken = [&context, &parseTargetToken](const std::string& token){
			auto parseTokenImpl = [&context, &parseTargetToken](const std::string& token, auto& parseTokenImpl) {
				auto alreadyHaveTarget = !context.target.empty();
				if (parseTargetToken(token))
					return !alreadyHaveTarget;

				auto kwit = Keyword::getMap().find(token);
				if (kwit != end(Keyword::getMap())) {
					if (std::visit(overloaded {
						[&context](eKillType kt) { if (context.killType == eKillType::Any) context.killType = kt; return true; },
						[&context](eKillMethod km) { if (context.killMethod == eKillMethod::NONE) context.killMethod = km; return true; },
						[&context](eMapKillMethod mkm) { if (context.mapMethod == eMapKillMethod::NONE) context.mapMethod = mkm; return true; },
						[&context](eKillComplication kc) { if (context.complication == eKillComplication::None) context.complication = kc; return true; },
					}, kwit->second)) return true;
				}

				if (context.killMethod == eKillMethod::NONE) {
					auto it = ::methodKeywords.find(token);
					if (it != end(::methodKeywords)) {
						return parseTokenImpl(it->second, parseTokenImpl);
					}
				}

				if (context.mission == eMission::NONE) return false;

				for (auto& dkws : disguiseKeywords) {
					if (!dkws.keywords.empty() && !dkws.missions.contains(context.mission)) continue;
					auto kwd = dkws.keywords.find(token);
					if (kwd != end(dkws.keywords)) {
						context.disguise = kwd->second;
						return true;
					}
				}
				return false;
			};
			return parseTokenImpl(token, parseTokenImpl);
		};

		if (context.tokens.empty() && context.conditions == 0) {
			auto token = std::string{};

			for (auto c : processed) {
				if (!isspace(c)) {
					if (isalnum(c)) token += c;
					continue;
				}

				if (token.empty()) continue;

				auto it = ::keywordKeywords.find(token);

				if (it != end(::keywordKeywords))
					context.tokens.push_back(it->second);
				else
					context.tokens.push_back(move(token));

				token = "";
			}

			if (!token.empty())
				context.tokens.push_back(move(token));
		}

		size_t i = context.nextIndex;

		if (context.mission == eMission::NONE) {
			auto token = std::string{};

			while (i < context.tokens.size()) {
				auto tokensLeft = context.tokens.size() - i;
				
				switch (tokensLeft) {
				default:
				case 3:
					token = context.tokens[i] + context.tokens[i + 1] + context.tokens[i + 2];
					if (parseTargetToken(token)) {
						context.tokens.erase(begin(context.tokens) + i, end(context.tokens) + i + 3);
						break;
					}
					[[fallthrough]];
				case 2:
					token = context.tokens[i] + context.tokens[i + 1];
					if (parseTargetToken(token)) {
						context.tokens.erase(begin(context.tokens) + i, end(context.tokens) + i + 2);
						break;
					}
					[[fallthrough]];
				case 1:
					if (parseTargetToken(context.tokens[i])) {
						context.tokens.erase(begin(context.tokens) + i);
						break;
					}
					++i;
					continue;
				case 0: break;
				}
				break;
			}

			if (context.mission == eMission::NONE)
				context.mission = eMission::BERLIN_APEXPREDATOR;
		}

		for (size_t i = 0; i < context.tokens.size(); ) {
			auto tokensLeft = context.tokens.size() - i;
			switch (tokensLeft) {
			default:
			case 4:
				if (parseToken(context.tokens[i] + context.tokens[i+1] + context.tokens[i+2] + context.tokens[i + 3])) {
					context.tokens.erase(begin(context.tokens) + i, begin(context.tokens) + i + 4);
					break;
				}
				[[fallthrough]];
			case 3:
				if (parseToken(context.tokens[i] + context.tokens[i+1] + context.tokens[i+2])) {
					context.tokens.erase(begin(context.tokens) + i, begin(context.tokens) + i + 3);
					break;
				}
				[[fallthrough]];
			case 2:
				if (parseToken(context.tokens[i] + context.tokens[i+1])) {
					context.tokens.erase(begin(context.tokens) + i, begin(context.tokens) + i + 2);
					break;
				}
				[[fallthrough]];
			case 1:
				if (parseToken(context.tokens[i])) {
					context.tokens.erase(begin(context.tokens) + i);
					break;
				}
				else ++i;
				break;
			case 0: break;
			}

			if (
				(!context.target.empty() || context.mission == eMission::BERLIN_APEXPREDATOR)
				&& !context.disguise.empty()
				&& (context.killMethod != eKillMethod::NONE || context.mapMethod != eMapKillMethod::NONE)
			)
				break;
		}

		context.nextIndex = i;
		return !context.target.empty()
			&& !context.disguise.empty()
			&& (context.killMethod != eKillMethod::NONE || context.mapMethod != eMapKillMethod::NONE);
	}

private:

};
