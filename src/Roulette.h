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
#include <vector>
#include "RouletteRuleset.h"

enum class eMission {
	NONE,
	ICAFACILITY_ARRIVAL,
	ICAFACILITY_GUIDED,
	ICAFACILITY_FREEFORM,
	ICAFACILITY_FINALTEST,
	PARIS_SHOWSTOPPER,
	PARIS_HOLIDAYHOARDERS,
	SAPIENZA_WORLDOFTOMORROW,
	SAPIENZA_THEICON,
	SAPIENZA_LANDSLIDE,
	SAPIENZA_THEAUTHOR,
	MARRAKESH_GILDEDCAGE,
	MARRAKESH_HOUSEBUILTONSAND,
	BANGKOK_CLUB27,
	BANGKOK_THESOURCE,
	COLORADO_FREEDOMFIGHTERS,
	//COLORADO_THEVECTOR,
	HOKKAIDO_SITUSINVERSUS,
	HOKKAIDO_SNOWFESTIVAL,
	HOKKAIDO_PATIENTZERO,
	HAWKESBAY_NIGHTCALL,
	MIAMI_FINISHLINE,
	MIAMI_ASILVERTONGUE,
	SANTAFORTUNA_THREEHEADEDSERPENT,
	SANTAFORTUNA_EMBRACEOFTHESERPENT,
	MUMBAI_CHASINGAGHOST,
	MUMBAI_ILLUSIONSOFGRANDEUR,
	WHITTLETON_ANOTHERLIFE,
	WHITTLETON_ABITTERPILL,
	ISLEOFSGAIL_THEARKSOCIETY,
	NEWYORK_GOLDENHANDSHAKE,
	HAVEN_THELASTRESORT,
	DUBAI_ONTOPOFTHEWORLD,
	DARTMOOR_DEATHINTHEFAMILY,
	BERLIN_APEXPREDATOR,
	CHONGQING_ENDOFANERA,
	MENDOZA_THEFAREWELL,
	CARPATHIAN_UNTOUCHABLE,
	AMBROSE_SHADOWSINTHEWATER,
};

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

enum class eMapKillMethod {
	NONE,
	AmputationKnife,
	AntiqueCurvedKnife,
	BarberRazor,
	BattleAxe,
	BeakStaff,
	Broadsword,
	BurialKnife,
	CircumcisionKnife,
	ConcealableKnife,
	CombatKnife,
	Cleaver,
	FireAxe,
	FoldingKnife,
	GardenFork,
	GrapeKnife,
	Hatchet,
	HobbyKnife,
	Hook,
	Icicle,
	JarlsPirateSaber,
	Katana,
	KitchenKnife,
	KukriMachete,
	LetterOpener,
	Machete,
	MeatFork,
	OldAxe,
	OrnateScimitar,
	RustyScrewdriver,
	Saber,
	SacrificialKnife,
	SappersAxe,
	Scalpel,
	Scissors,
	ScrapSword,
	Screwdriver,
	Seashell,
	Shears,
	Shuriken,
	Starfish,
	Tanto,
	UnicornHorn,
	VikingAxe,

	Soders_Electrocution,
	Soders_Explosion,
	Soders_PoisonStemCells,
	Soders_RobotArms,
	Soders_ShootHeart,
	Soders_TrashHeart,
	Yuki_SabotageCableCar,
};

enum class eKillType {
	Any,
	Silenced,
	Loud,
	Melee,
	Thrown,
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

enum class eTargetType {
	Normal,
	Soders,
};
struct KillMethod;
struct MapKillMethod;
class RouletteSpinCondition;
class RouletteSpinGenerator;

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

struct MapKillMethod
{
	eMapKillMethod method;
	std::string_view name;
	std::string_view image;
	bool isMelee = false;
	bool isLoadout = false;

	MapKillMethod(eMapKillMethod method);
	MapKillMethod(MapKillMethod&&) noexcept = default;
	MapKillMethod(const MapKillMethod&) = default;
	auto operator=(MapKillMethod&&) noexcept -> MapKillMethod& = default;
};

class RouletteSpinMethod
{
public:
	RouletteSpinMethod(eKillMethod killMethod) : killMethod(killMethod)
	{}

private:
	eKillMethod killMethod;
};

struct RouletteDisguise
{
public:
	RouletteDisguise(std::string name, std::string image, bool suit = false) : name(name), image(image), suit(suit)
	{}

public:
	std::string name;
	std::string image;
	bool suit = false;
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

class RouletteMission
{
public:
	RouletteMission(eMission mission) : mission(mission) {}

	auto getMission() const { return this->mission; }
	auto& getDisguises() const { return this->disguises; }
	auto& getTargets() const { return this->targets; }
	auto& getMapKillMethods() const { return this->mapKillMethods; }
	auto getObjectiveCount() const { return this->targets.size(); }

	auto getDisguiseByName(std::string_view name) const {
		auto it = std::find_if(this->disguises.cbegin(), this->disguises.cend(), [name](const RouletteDisguise& disguise) {
			return disguise.name == name;
		});
		return it != this->disguises.end() ? &*it : nullptr;
	}

	auto& getDisguiseByNameAssert(std::string_view name) const {
		auto disguise = this->getDisguiseByName(name);
		if (!disguise) throw RouletteGeneratorException(std::format("Failed to find disguise \"{}\".", name));
		return *disguise;
	}

	auto& addTarget(std::string name, std::string image, eTargetType type = eTargetType::Normal) {
		this->targets.emplace_back(name, image, type);
		return this->targets.back();
	}

	auto& addDisguise(std::string name, std::string image, bool suit = false) {
		this->disguises.emplace_back(name, image, suit);
		return this->disguises.back();
	}

	auto& addMapMethod(eMapKillMethod method) {
		this->mapKillMethods.emplace_back(method);
		return this->mapKillMethods.back();
	}

private:
	eMission mission = eMission::NONE;
	std::vector<RouletteTarget> targets;
	std::vector<RouletteDisguise> disguises;
	std::vector<MapKillMethod> mapKillMethods;
};

struct RouletteSpinCondition
{
	std::reference_wrapper<const RouletteTarget> target;
	std::reference_wrapper<const RouletteDisguise> disguise;
	KillMethod killMethod;
	MapKillMethod specificKillMethod;
	eKillType killType = eKillType::Any;
	eKillComplication killComplication = eKillComplication::None;
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

	auto allowBannedInRR(bool allow) {
		this->bannedInRRConditionsAllowed = allow;
	}

	auto allowImpossible(bool allow) {
		this->impossibleConditionsAllowed = allow;
	}

	auto allowBuggy(bool allow) {
		this->buggyConditionsAllowed = allow;
	}

	auto spin() {
		static auto methodTypes = std::vector<eMethodType>{
			eMethodType::Standard,
			eMethodType::Map,
			eMethodType::Gun,
		};

		RouletteSpin spin(this->mission);
		std::set<eKillMethod> usedMethods;
		std::set<eMapKillMethod> usedMapMethods;

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
				if (spin.hasDisguise(disguise) && !this->duplicateDisguiseAllowed) continue;

				auto cond = std::optional<RouletteSpinCondition>();

				if (target.getType() == eTargetType::Soders) {
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

					if (alreadySpanMethod && !this->duplicateKillMethodAllowed) continue;

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

					if (alreadySpanMethod && !this->duplicateKillMethodAllowed) continue;

					auto& tags = target.getMethodTags(killMethod);
					auto killInfo = KillMethod{killMethod};

					if (this->doTagsViolateRules(tags)) continue;

					auto killType = eKillType::Any;
					if (killInfo.isGun) killType = randomVectorElement(this->gunKillTypes);
					else if (killMethod == eKillMethod::Explosive) killType = randomVectorElement(this->explosiveKillTypes);
					else if (useSpecificMethod && mapMethodInfo.isMelee) killType = randomVectorElement(meleeKillTypes);

					// 20% chance of 'Live' kill complication
					auto canHaveLiveKill = this->rules->liveComplications && (
						(methodType == eMethodType::Standard && !this->rules->liveComplicationsExcludeStandard)
						|| ((useSpecificMethod && mapMethodInfo.isMelee) || isKillMethodLivePrefixable(killMethod))
					);
					auto killComplication = canHaveLiveKill && randomBool(20) ? eKillComplication::Live : eKillComplication::None;

					cond.emplace(target, disguise, std::move(killInfo), std::move(mapMethodInfo), killType, killComplication);
				}

				if (cond) {
					if (spin.getNumLargeFirearms() > 0 && cond->killMethod.isGun && cond->killMethod.isLarge) continue;
					if (cond->killMethod.method == eKillMethod::Explosive && cond->killType == eKillType::Loud)
						cond->killMethod.isRemote = false;

					auto tags = target.testRules(*cond);

					if (this->doTagsViolateRules(tags)) continue;

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
		return (tags.contains(eMethodTag::Buggy) && !this->buggyConditionsAllowed)
			|| (tags.contains(eMethodTag::BannedInRR) && !this->bannedInRRConditionsAllowed)
			|| (tags.contains(eMethodTag::Hard) && !this->hardConditionsAllowed)
			|| (tags.contains(eMethodTag::Extreme) && !this->extremeConditionsAllowed)
			|| (tags.contains(eMethodTag::Impossible) && !this->impossibleConditionsAllowed);
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
	bool buggyConditionsAllowed = false;
	bool liveKillTypeAllowed = false;
	bool eliminationsAllowed = false;
	bool specificEliminationsAllowed = false;
	bool bannedInRRConditionsAllowed = false;
	bool hardConditionsAllowed = false;
	bool extremeConditionsAllowed = false;
	bool impossibleConditionsAllowed = false;
	bool duplicateDisguiseAllowed = false;
	bool duplicateKillMethodAllowed = false;
};
