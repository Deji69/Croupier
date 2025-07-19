#pragma once
#include <optional>
#include <string>
#include <string_view>
#include <unordered_map>
#include "Disguise.h"
#include "Exception.h"
#include "Target.h"
#include "KillMethod.h"

enum class eMission;
enum class eMapKillMethod;
struct MapKillMethod;
class RouletteTarget;

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

struct MapKillMethod {
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

struct MissionInfo {
	eMission mission;
	std::string_view name;
	std::string_view simpleName;
	bool isMainMap = false;

	MissionInfo(eMission mission, std::string_view name, std::string_view simpleName, bool isMainMap = true) :
		mission(mission), name(name), simpleName(simpleName), isMainMap(isMainMap)
	{ }
};

extern const std::vector<eMission> defaultMissionPool;
extern const std::unordered_map<std::string, eMission> missionsByCodename;
extern const std::unordered_map<std::string, eMission> missionsByContractId;
extern const std::unordered_map<eMission, std::vector<MapKillMethod>> missionMethods;
extern const std::unordered_map<eMission, std::vector<RouletteDisguise>> missionDisguises;
extern const std::vector<MissionInfo> missionInfos;
extern const RouletteDisguise anyDisguise;

inline auto& getMissionMethods(eMission mission) {
	static const std::vector<MapKillMethod> emptyMissionMethods;
	auto it = missionMethods.find(mission);
	if (it == end(missionMethods)) return emptyMissionMethods;
	return it->second;
}

inline auto& getMissionDisguises(eMission mission) {
	static const std::vector<RouletteDisguise> emptyDisguises;
	auto it = missionDisguises.find(mission);
	if (it == end(missionDisguises)) return emptyDisguises;
	return it->second;
}

inline auto getMissionByCodename(const std::string& codename) {
	auto it = missionsByCodename.find(codename);
	if (it != missionsByCodename.end()) return it->second;
	return eMission::NONE;
}

inline auto getMissionByContractId(const std::string& str) {
	auto it = missionsByContractId.find(str);
	if (it == missionsByContractId.end()) return eMission::NONE;
	return it->second;
}

inline auto getMissionCodename(eMission mission) -> std::optional<std::string_view> {
	auto it = find_if(cbegin(missionsByCodename), cend(missionsByCodename), [mission](const std::pair<std::string, eMission>& v) {
		return v.second == mission;
	});
	if (it == cend(missionsByCodename)) return std::nullopt;
	return std::make_optional<std::string_view>(it->first);
}

class RouletteMission
{
public:
	RouletteMission(eMission mission);

	inline auto getMission() const { return this->mission; }
	inline auto& getDisguises() const { return this->disguises; }
	inline auto& getTargets() const { return this->targets; }
	inline auto& getMapKillMethods() const { return this->mapKillMethods; }

	auto getObjectiveCount() const -> size_t;

	auto getSuitDisguise() const {
		auto it = find_if(cbegin(this->disguises), cend(this->disguises), [](const RouletteDisguise& disguise) { return disguise.suit; });
		return it != cend(this->disguises) ? &*it : nullptr;
	}

	auto getDisguiseByName(std::string_view name) const {
		if (name == "Any Disguise") return &anyDisguise;
		auto it = find_if(cbegin(this->disguises), cend(this->disguises), [name](const RouletteDisguise& disguise) {
			return disguise.name == name;
		});
		return it != cend(this->disguises) ? &*it : nullptr;
	}

	auto& getDisguiseByNameAssert(std::string_view name) const {
		auto disguise = this->getDisguiseByName(name);
		if (!disguise) throw RouletteGeneratorException(std::format("Failed to find disguise \"{}\".", name));
		return *disguise;
	}

	auto getTargetByName(std::string_view name) const -> const RouletteTarget*;

	auto addTarget(eTargetID id, std::string name, std::string image, eTargetType type = eTargetType::Normal) -> RouletteTarget&;

private:
	eMission mission = eMission::NONE;
	std::vector<RouletteTarget> targets;
	const std::vector<RouletteDisguise>& disguises;
	const std::vector<MapKillMethod>& mapKillMethods;
};

struct Missions {
	Missions();
	static auto get(eMission mission) -> const RouletteMission*;

private:
	static Missions instance;
	static std::vector<RouletteMission> data;
};
