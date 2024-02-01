#pragma once
#include <array>
#include <exception>
#include <format>
#include <functional>
#include <initializer_list>
#include <optional>
#include <random>
#include <set>
#include <string>
#include <unordered_map>
#include <unordered_set>
#include <variant>
#include <vector>
#include "Disguise.h"
#include "Exception.h"
#include "Target.h"
#include "RouletteMission.h"
#include "RouletteRuleset.h"
#include "util.h"

enum class eMethodType {
	Map,
	Gun,
	Standard,
};

enum class eKillMethod {
	NONE,
	Pistol,
	SMG,
	Sniper,
	Shotgun,
	AssaultRifle,
	Elimination,
	PistolElimination,
	SMGElimination,
	Explosive,
	Drowning,
	FallingObject,
	Fall,
	Fire,
	Electrocution,
	Explosion,
	ConsumedPoison,
	InjectedPoison,
	FiberWire,
	NeckSnap,
};

enum class eKillType {
	Any,
	Silenced,
	Loud,
	Melee,
	Thrown,
	Remote,
	Impact,
	LoudRemote,
};

enum class eKillComplication {
	None,
	Live,
};

enum class eMethodTag {
	BannedInRR,
	Impossible,
	Hard,
	Extreme,
	Buggy,

	DuplicateOnlySameDisguise,
	LoudOnly,

	IsGun,
	IsElimination,
	IsLarge,
	IsRemote,
};

struct KillMethod;
struct MapKillMethod;
class RouletteSpinCondition;
class RouletteSpinGenerator;

auto isMethodTagHigherDifficulty(eMethodTag a, eMethodTag b) -> bool;
auto isKillMethodGun(eKillMethod) -> bool;
auto isKillMethodLarge(eKillMethod) -> bool;
auto isKillMethodRemote(eKillMethod) -> bool;
auto isKillMethodElimination(eKillMethod) -> bool;
auto isKillMethodLivePrefixable(eKillMethod) -> bool;
auto isSpecificKillMethodMelee(eMapKillMethod) -> bool;

auto getKillTypeName(eKillType) -> std::string_view;
auto getKillMethodName(eKillMethod) -> std::string_view;
auto getKillMethodImage(eKillMethod) -> std::string_view;
auto getSpecificKillMethodName(eMapKillMethod) -> std::string_view;
auto getSpecificKillMethodImage(eMapKillMethod) -> std::string_view;
auto getKillComplicationName(eKillComplication) -> std::string_view;

auto getMissionForTarget(const std::string& targetName) -> eMission;

struct Keyword
{
public:
	using Variant = std::variant<eKillType, eKillMethod, eKillComplication, eMapKillMethod>;
	static std::vector<Keyword> keywords;

private:
	static std::unordered_map<std::string, Variant> keywordMap;
	static std::unordered_map<std::string, std::string> targetKeyMap;
	static std::unordered_map<std::string, std::string> keyTargetMap;

public:
	std::string keyword;
	Variant value;
	std::string alias = "";

	Keyword(std::string keyword, eKillType value, std::string alias = "") : keyword(keyword), value(value), alias(alias)
	{ }
	Keyword(std::string keyword, eKillMethod value, std::string alias = "") : keyword(keyword), value(value), alias(alias)
	{ }
	Keyword(std::string keyword, eMapKillMethod value, std::string alias = "") : keyword(keyword), value(value), alias(alias)
	{ }
	Keyword(std::string keyword, Variant value, std::string alias = "") : keyword(keyword), value(value), alias(alias)
	{ }

	auto convertToSodersKill() {
		if (auto killMethod = std::get_if<eKillMethod>(&this->value)) {
			switch (*killMethod) {
			case eKillMethod::Electrocution: return eMapKillMethod::Soders_Electrocution;
			case eKillMethod::Explosion: return eMapKillMethod::Soders_Explosion;
			case eKillMethod::ConsumedPoison: return eMapKillMethod::Soders_PoisonStemCells;
			}
		}
		return eMapKillMethod::NONE;
	}

	static auto convertFromSodersKill(eMapKillMethod killMethod) -> Variant {
		switch (killMethod) {
		case eMapKillMethod::Soders_Electrocution: return eKillMethod::Electrocution;
		case eMapKillMethod::Soders_Explosion: return eKillMethod::Explosion;
		case eMapKillMethod::Soders_PoisonStemCells: return eKillMethod::ConsumedPoison;
		}
		return killMethod;
	}

	static auto& getMap() {
		if (keywordMap.empty()) {
			for (auto& keyword : keywords)
				keywordMap.emplace(toLowerCase(keyword.keyword), keyword.value);
		}
		return keywordMap;
	}

	static auto get(Variant method) -> std::string_view {
		if (std::holds_alternative<eMapKillMethod>(method))
			method = convertFromSodersKill(std::get<eMapKillMethod>(method));
		for (auto& keyword : keywords) {
			if (!keyword.alias.empty()) continue;
			if (keyword.value != method) continue;
			return keyword.keyword;
		}
		return "";
	}

	static auto getForTarget(const std::string& name) -> std::string_view {
		auto it = targetKeyMap.find(name);
		if (it != end(targetKeyMap)) return it->second;
		return "";
	}

	static auto targetKeyToName(std::string_view key) -> std::string_view {
		for (auto& target : targetKeyMap) {
			if (target.second == key) return target.first;
		}
		return "";
	}
};

struct KillMethod
{
	eKillMethod method;
	std::string_view name;
	std::string_view image;
	bool isGun = false;
	bool isLarge = false;
	bool isRemote = false;
	bool isElimination = false;

	KillMethod(eKillMethod method);
	KillMethod(KillMethod&&) noexcept = default;
	KillMethod(const KillMethod&) = default;
	auto operator=(KillMethod&&) noexcept -> KillMethod& = default;
};

class RouletteSpinMethod
{
public:
	RouletteSpinMethod(eKillMethod killMethod) : killMethod(killMethod)
	{}

private:
	eKillMethod killMethod;
};

class RouletteTarget
{
public:
	RouletteTarget(std::string name, std::string image, eTargetType type = eTargetType::Normal) : name(name), image(image), type(type)
	{}

	auto& getName() const { return this->name; }
	auto& getImage() const { return this->image; }
	auto getType() const { return this->type; }

	auto addRule(std::function<bool(const RouletteSpinCondition& cond)> fn, std::initializer_list<eMethodTag> tags = {}) {
		this->rules.emplace_back(std::move(fn), tags);
	}

	auto testRules(const RouletteSpinCondition& cond) const {
		auto broken = std::set<eMethodTag>{};
		for (auto& rule : this->rules) {
			if ((rule.first)(cond)) {
				for (auto const tag : rule.second) {
					broken.emplace(tag);
				}
			}
		}
		return broken;
	}

	auto defineMethod(eKillMethod method, std::initializer_list<eMethodTag> tags = {}) {
		auto it = this->methodInfo.find(method);
		if (it == this->methodInfo.end())
			this->methodInfo.emplace(method, tags);
		else {
			for (auto tag : tags)
				it->second.emplace(tag);
		}
	}

	auto defineMethod(eMapKillMethod method, std::initializer_list<eMethodTag> tags = {}) {
		auto it = this->specialMethodInfo.find(method);
		if (it == this->specialMethodInfo.end())
			this->specialMethodInfo.emplace(method, tags);
		else {
			for (auto tag : tags)
				it->second.emplace(tag);
		}
	}

	auto getMethodTags(eKillMethod method) const -> const std::set<eMethodTag>& {
		auto it = this->methodInfo.find(method);
		if (it != this->methodInfo.end()) return it->second;
		return RouletteTarget::emptyMethodTags;
	}

	auto getMethodTags(eMapKillMethod method) const -> const std::set<eMethodTag>& {
		auto it = this->specialMethodInfo.find(method);
		if (it != this->specialMethodInfo.end()) return it->second;
		return RouletteTarget::emptyMethodTags;
	}

	auto isMethodTagged(eKillMethod method, eMethodTag tag) const {
		auto it = this->methodInfo.find(method);
		if (it != this->methodInfo.end())
			return it->second.contains(tag);
		return false;
	}

	auto isMethodTagged(eMapKillMethod method, eMethodTag tag) const {
		auto it = this->specialMethodInfo.find(method);
		if (it != this->specialMethodInfo.end())
			return it->second.contains(tag);
		return false;
	}

private:
	static std::set<eMethodTag> emptyMethodTags;

	eTargetType type;
	std::string name;
	std::string image;
	std::vector<std::pair<std::function<bool(const RouletteSpinCondition&)>, std::set<eMethodTag>>> rules;
	std::unordered_map<eKillMethod, std::set<eMethodTag>> methodInfo;
	std::unordered_map<eMapKillMethod, std::set<eMethodTag>> specialMethodInfo;
};

struct RouletteSpinCondition
{
	std::reference_wrapper<const RouletteTarget> target;
	std::reference_wrapper<const RouletteDisguise> disguise;
	KillMethod killMethod;
	MapKillMethod specificKillMethod;
	eKillType killType = eKillType::Any;
	eKillComplication killComplication = eKillComplication::None;
	bool lockMethod = false;
	bool lockDisguise = false;
	std::string methodName;

	RouletteSpinCondition(
		const RouletteTarget& target, const RouletteDisguise& disguise,
		KillMethod killMethod, MapKillMethod specificKillMethod,
		eKillType killType = eKillType::Any, eKillComplication killComplication = eKillComplication::None
	) :
		target(target), disguise(disguise),
		killMethod(killMethod), specificKillMethod(specificKillMethod), killType(killType),
		killComplication(killComplication)
	{
		if (this->killMethod.name.empty()) this->killMethod.name = specificKillMethod.name;
		if (this->killMethod.image.empty()) this->killMethod.image = specificKillMethod.image;

		methodName = this->killMethod.name;

		if (killType != eKillType::Any)
			methodName = std::format("{} {}", getKillTypeName(killType), methodName);
		if (killComplication != eKillComplication::None)
			methodName = std::format("{} {}", getKillComplicationName(killComplication), methodName);
	}

	RouletteSpinCondition(RouletteSpinCondition&&) noexcept = default;

	auto operator=(RouletteSpinCondition&&) noexcept -> RouletteSpinCondition& = default;
};

class RouletteSpin
{
public:
	RouletteSpin() = default;
	RouletteSpin(RouletteSpin&& o) noexcept = default;
	RouletteSpin(const RouletteSpin&) = default;
	RouletteSpin(const RouletteMission* mission) : mission(mission) {}

	auto operator=(RouletteSpin&&) noexcept -> RouletteSpin& = default;

	auto& add(RouletteSpinCondition&& cond) {
		this->conditions.emplace_back(std::forward<RouletteSpinCondition>(cond));
		return this->conditions.back();
	}

	auto& getForTarget(const RouletteTarget& target) {
		auto it = std::find_if(this->conditions.begin(), this->conditions.end(), [&target](const RouletteSpinCondition& cond) {
			return &cond.target.get() == &target;
		});
		if (it == this->conditions.end())
			throw RouletteGeneratorException("Invalid target for current generator.");
		return *it;
	}

	auto setTargetDisguise(const RouletteTarget& target, const RouletteDisguise& disguise) {
		auto& cond = this->getForTarget(target);
		cond = RouletteSpinCondition{target, disguise, cond.killMethod, cond.specificKillMethod, cond.killType, cond.killComplication };
	}

	auto setTargetStandardMethod(const RouletteTarget& target, eKillMethod method) {
		auto& cond = this->getForTarget(target);
		cond = RouletteSpinCondition{target, cond.disguise, method, eMapKillMethod::NONE, eKillType::Any, cond.killComplication };
	}

	auto setTargetMapMethod(const RouletteTarget& target, eMapKillMethod method) {
		auto& cond = this->getForTarget(target);
		cond = RouletteSpinCondition{target, cond.disguise, eKillMethod::NONE, method, eKillType::Any, cond.killComplication };
	}

	auto setTargetMethodType(const RouletteTarget& target, eKillType killType) {
		auto& cond = this->getForTarget(target);
		cond = RouletteSpinCondition{target, cond.disguise, cond.killMethod, cond.specificKillMethod, killType, cond.killComplication};
	}

	auto setTargetComplication(const RouletteTarget& target, eKillComplication complication) {
		auto& cond = this->getForTarget(target);
		cond = RouletteSpinCondition{target, cond.disguise, cond.killMethod, cond.specificKillMethod, cond.killType, complication};
	}

	auto getMission() const {
		return this->mission;
	}

	auto getNumLargeFirearms() const {
		return std::count_if(this->conditions.cbegin(), this->conditions.cend(), [](const RouletteSpinCondition& v){
			return v.killMethod.isGun && v.killMethod.isLarge;
		});
	}

	auto countKillMethod(eKillMethod method) const {
		return std::count_if(this->conditions.cbegin(), this->conditions.cend(), [method](const RouletteSpinCondition& v) {
			return v.killMethod.method == method;
		});
	}

	auto hasDisguise(const RouletteDisguise& disguise) const {
		return std::find_if(this->conditions.cbegin(), this->conditions.cend(), [&disguise](const RouletteSpinCondition& c) {
			return &c.disguise.get() == &disguise;
		}) != this->conditions.end();
	}

	auto& getConditions() noexcept { return this->conditions; }
	auto const& getConditions() const noexcept { return this->conditions; }

private:
	const RouletteMission* mission = nullptr;
	std::vector<RouletteSpinCondition> conditions;
};

class RouletteSpinGenerator
{
public:
	static const std::vector<eKillMethod> standardKillMethods;
	static const std::vector<eKillType> gunKillTypes;
	static const std::vector<eKillType> explosiveKillTypes;
	static const std::vector<eKillType> ogExplosiveKillTypes;
	static const std::vector<eMapKillMethod> sodersKills;
	std::vector<eKillType> meleeKillTypes;
	std::vector<eKillMethod> firearmKillMethods;
	std::vector<eKillComplication> killComplications;

public:
	RouletteSpinGenerator() noexcept = default;

	auto getMission() { return this->mission; }

	auto setMission(const RouletteMission* mission) {
		this->mission = mission;
	}

	auto setRuleset(const RouletteRuleset* ruleset) {
		this->rules = ruleset;
		this->meleeKillTypes = {eKillType::Any};
		if (this->rules->meleeKillTypes) this->meleeKillTypes.emplace_back(eKillType::Melee);
		if (this->rules->thrownKillTypes) this->meleeKillTypes.emplace_back(eKillType::Thrown);
		this->killComplications = {eKillComplication::None};
		if (this->rules->liveComplications) this->killComplications.emplace_back(eKillComplication::Live);
		this->firearmKillMethods = {
			eKillMethod::AssaultRifle,
			eKillMethod::Pistol,
			eKillMethod::Shotgun,
			eKillMethod::SMG,
			eKillMethod::Sniper,
			//eKillMethod::PistolElimination,
			//eKillMethod::SMGElimination,
		};
		if (this->rules->genericEliminations) this->firearmKillMethods.emplace_back(eKillMethod::Elimination);
	}

	auto allowDuplicateDisguise(bool allow) {
		this->duplicateDisguiseAllowed = allow;
	}

	auto allowDuplicateKillMethods(bool allow) {
		this->duplicateKillMethodAllowed = allow;
	}

	auto spin(RouletteSpin* existing = nullptr, bool useExistingDisguise = false, bool useExistingCondition = false) {
		static auto methodTypes = std::vector<eMethodType>{
			eMethodType::Standard,
			eMethodType::Map,
			eMethodType::Gun,
		};

		RouletteSpin spin(this->mission);
		std::set<eKillMethod> usedMethods;
		std::set<eMapKillMethod> usedMapMethods;

		//if (!existing) useExistingDisguise = useExistingCondition = false;

		auto& targets = this->mission->getTargets();
		auto& disguises = this->mission->getDisguises();
		auto& mapKillMethods = this->mission->getMapKillMethods();

		for (const auto& target : targets) {
			for (auto attempts = 0; ; ++attempts) {
				if (attempts > 30) throw RouletteGeneratorException("Failed to generate spin.");

				auto methodType = randomVectorElement(methodTypes);
				while (!mapKillMethods.size() && methodType == eMethodType::Map)
					methodType = randomVectorElement(methodTypes);

				auto useSpecificMethod = methodType == eMethodType::Map;
				auto& disguise = randomVectorElement(disguises);
				if (spin.hasDisguise(disguise) && !this->rules->allowDuplicateDisguise) continue;

				auto cond = std::optional<RouletteSpinCondition>();

				if (useExistingCondition) {
					//cond.emplace(target, disguise, existing->killMethod, existing->specificKillMethod, existing->killType, existing->killComplication);
				}
				else if (target.getType() == eTargetType::Soders) {
					auto killMethod = !useSpecificMethod ? randomVectorElement(firearmKillMethods) : eKillMethod::NONE;
					auto specificMethod = useSpecificMethod ? randomVectorElement(sodersKills) : eMapKillMethod::NONE;
					auto killInfo = KillMethod{killMethod};
					auto mapMethodInfo = MapKillMethod{specificMethod};
					if (killInfo.isElimination) continue;
					auto killType = killInfo.isGun
						? randomVectorElement(gunKillTypes)
						: eKillType::Any;
					auto alreadySpanMethod = useSpecificMethod
						? false
						: usedMethods.contains(killMethod);

					if (alreadySpanMethod && !this->rules->allowDuplicateMethod) continue;

					cond.emplace(target, disguise, std::move(killInfo), std::move(mapMethodInfo), killType);
				}
				else if (target.getType() == eTargetType::Normal) {
					auto mapMethodInfo = useSpecificMethod ? randomVectorElement(this->mission->getMapKillMethods()) : eMapKillMethod::NONE;
					auto killMethod = useSpecificMethod
						? eKillMethod::NONE
						: (methodType == eMethodType::Standard
							? randomVectorElement(standardKillMethods)
							: randomVectorElement(firearmKillMethods));
					auto alreadySpanMethod = useSpecificMethod
						? usedMapMethods.contains(mapMethodInfo.method)
						: usedMethods.contains(killMethod);

					if (alreadySpanMethod && !this->rules->allowDuplicateMethod) continue;

					auto killType = eKillType::Any;
					auto killInfo = KillMethod{killMethod};

					if (killInfo.isGun) killType = randomVectorElement(this->gunKillTypes);
					else if (killMethod == eKillMethod::Explosive) killType = randomVectorElement(this->ogExplosiveKillTypes);
					else if (useSpecificMethod && mapMethodInfo.isMelee) killType = randomVectorElement(meleeKillTypes);

					//
					auto& tags = target.getMethodTags(killMethod);
					if (this->doTagsViolateRules(tags)) continue;

					// Check live complications are enabled, if we have a standard kill check it's prefixable, then check more stuff...
					auto canHaveLiveKill = this->rules->liveComplications && (killMethod == eKillMethod::NONE || isKillMethodLivePrefixable(killMethod)) && (
						// Allow 'standard' kills if the exclude option is not enabled
						(methodType == eMethodType::Standard && !this->rules->liveComplicationsExcludeStandard)
						// Neck snaps are the exception 'standard' kill allowed as it's also 'melee'
						|| (methodType == eMethodType::Standard && killMethod == eKillMethod::NeckSnap)
						// Allow all firearm kills
						|| (methodType == eMethodType::Gun)
						// Allow all specific/map melee kills
						|| (useSpecificMethod && mapMethodInfo.isMelee)
					);

					auto killComplication = canHaveLiveKill && randomBool(this->rules->liveComplicationChance)
						? eKillComplication::Live
						: eKillComplication::None;

					cond.emplace(target, disguise, std::move(killInfo), std::move(mapMethodInfo), killType, killComplication);
				}

				if (cond) {
					if (!useExistingCondition) {
						if (spin.getNumLargeFirearms() > 0 && cond->killMethod.isGun && cond->killMethod.isLarge)
							continue;
					}

					if (cond->killMethod.method == eKillMethod::Explosive && cond->killType == eKillType::Loud)
						cond->killMethod.isRemote = false;

					auto tags = target.testRules(*cond);

					if (!useExistingCondition && !useExistingDisguise && this->doTagsViolateRules(tags)) continue;

					if (useSpecificMethod) {
						cond->killMethod.name = cond->specificKillMethod.name;
						cond->killMethod.image = cond->specificKillMethod.image;
						if (target.getType() != eTargetType::Soders) usedMapMethods.emplace(cond->specificKillMethod.method);
					}
					else if (target.getType() != eTargetType::Soders) usedMethods.emplace(cond->killMethod.method);

					spin.add(std::move(*cond));
					break;
				}
			}
		}

		return spin;
	}

	auto doTagsViolateRules(const std::set<eMethodTag>& tags) -> bool {
		if (tags.empty()) return false;
		return (tags.contains(eMethodTag::Buggy) && !this->rules->enableBuggy)
			|| (tags.contains(eMethodTag::BannedInRR) && !this->rules->enableMedium)
			|| (tags.contains(eMethodTag::Hard) && !this->rules->enableHard)
			|| (tags.contains(eMethodTag::Extreme) && !this->rules->enableExtreme)
			|| (tags.contains(eMethodTag::Impossible) && !this->rules->enableImpossible);
	}

private:
	static std::random_device rd;
	static std::mt19937 gen;

	template<typename T>
	static auto randomVectorElement(const std::vector<T>& vec) -> const T& {
		std::uniform_int_distribution<> dist(0, vec.size() - 1);
		return vec[dist(gen)];
	}

	template<typename T>
	static auto randomVectorIndex(const std::vector<T>& vec) -> int {
		std::uniform_int_distribution<> dist(0, vec.size() - 1);
		return dist(gen);
	}

	static auto randomBool() -> bool {
		std::uniform_int_distribution<> dist(0, 1);
		return dist(gen) != 0;
	}

	static auto randomBool(int percentage) -> bool {
		std::uniform_int_distribution<> dist(0, 100);
		return dist(gen) <= percentage;
	}

private:
	const RouletteRuleset* rules = nullptr;
	const RouletteMission* mission = nullptr;
	bool duplicateDisguiseAllowed = false;
	bool duplicateKillMethodAllowed = false;
};
