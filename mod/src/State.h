#pragma once
#include "Bingo.h"
#include "CroupierClient.h"
#include "Events.h"
#include "KillConfirmation.h"
#include "Roulette.h"
#include "RouletteRuleset.h"
#include <Glacier/Enums.h>
#include <Glacier/ZActor.h>
#include <Glacier/ZMath.h>
#include <Glacier/ZPrimitives.h>
#include <chrono>
#include <functional>
#include <map>
#include <optional>
#include <set>
#include <shared_mutex>
#include <stack>
#include <string>
#include <string_view>
#include <vector>
#include "json.hpp"

namespace Croupier
{
	enum class GameMode {
		Roulette,
		Bingo,
		Hybrid,
	};

	enum class PlayerStance {
		Unknown,
		Crouching,
		Standing,
	};

	enum class PlayerMoveType {
		Idle = 0,
		CrouchWalking = 1,
		CrouchRunning = 2,
		Walking = 3,
		Running = 4,
	};

	struct Area {
		std::string ID;
		SVector3 From;
		SVector3 To;
	};

	struct ActorData {
		const TEntityRef<ZActor>* actor = nullptr;
		const Area* area = nullptr;
		std::optional<ZRepositoryID> repoId = std::nullopt;
		std::optional<ZRepositoryID> disguiseRepoId = std::nullopt;
		SMatrix43 transform;
		EActorType actorType = EActorType::eAT_Last;
		EOutfitType outfitType = EOutfitType::eOT_None;
		int16_t roomId = -1;
		bool hasDisguise = false;
		bool isDead = false;
		bool isFemale = false;
		bool isPacified = false;
		bool isTarget = false;
	};

	struct KillSetpieceEvent {
		std::string id;
		std::string name;
		std::string type;
		double timestamp;
	};

	struct LevelSetupEvent {
		std::string event;
		double timestamp;
	};

	struct GameplayData {
		struct DisguiseChangeData {
			bool havePinData = false;
			bool haveEventData = false;
			bool wasFree = true;
			nlohmann::json eventData;
		};

		DisguiseChangeData disguiseChange;
		float4 playerBodyShotPos;
		bool playerIsDragging = false;
		bool sentPlayerDraggingEvent = false;
	};

	struct Commands {
		static std::function<void(bool)> Respin;
		static std::function<void()> Random;
		static std::function<void()> PreviousSpin;
	};

	struct State {
		static State current;

		mutable std::shared_mutex stateMutex;

		RouletteSpin spin;
		BingoCard card;
		RouletteSpinGenerator generator;
		CroupierClient client;
		RouletteRuleset rules;

		std::stack<RouletteSpin> spinHistory;
		std::map<ZRepositoryID, uint16_t> actorDataRepoIdMap;
		std::set<std::string, InsensitiveCompareLexicographic> killed;
		std::set<std::string, InsensitiveCompareLexicographic> spottedNotKilled;
		std::set<uintptr_t> collectedItemInstances;
		std::vector<DisguiseChange> disguiseChanges;
		std::vector<KillConfirmation> killValidations;
		std::vector<KillSetpieceEvent> killSetpieceEvents;
		std::vector<LevelSetupEvent> levelSetupEvents;
		std::vector<LoadoutItemEventValue> loadout;
		std::vector<Area> areas;
		std::array<ActorData, 1000> actorData;
		std::size_t actorDataSize = 0;
		std::string locationId;
		std::chrono::steady_clock::time_point timeStarted;
		std::chrono::seconds timeElapsed = std::chrono::seconds(0);
		eRouletteRuleset ruleset = eRouletteRuleset::RRWC2023;
		GameMode gameMode = GameMode::Roulette;
		double startIGT = 0;
		double exitIGT = 0;
		bool isSA = true;
		bool isCaughtOnCams = false;
		bool isCamsDestroyed = false;
		bool isPlaying = false;
		bool isFinished = false;
		bool hasLoadedGame = false;		// current play session is from a loaded game
		LONG windowX = 0;
		LONG windowY = 0;
		SMatrix43 playerMatrix;
		PlayerMoveType playerMoveType;
		bool playerInInstinct = false;
		bool playerInInstinctSinceFrame = false;
		bool playerStartingAgility = false;
		bool playerStartingAgilitySinceFrame = false;
		bool playerShooting = false;
		bool playerShootingSinceFrame = false;
		bool isTrespassing = false;
		const Area* area = nullptr;
		int16_t roomId = -1;

		bool spinCompleted = false;
		bool spinLocked = false;

		State() : timeElapsed(0) {
			timeStarted = std::chrono::steady_clock().now();
			this->resetKillValidations();
		}

		auto reset() -> void {
			killed.clear();
			spottedNotKilled.clear();
			disguiseChanges.clear();
			killSetpieceEvents.clear();
			levelSetupEvents.clear();
			collectedItemInstances.clear();
			actorDataRepoIdMap.clear();

			roomId = -1;
			playerMoveType = PlayerMoveType::Idle;
			isTrespassing = false;
			playerInInstinct = false;
			playerShooting = false;
			area = nullptr;

			for (auto& actorData : this->actorData)
				actorData = ActorData{};

			this->actorDataSize = 0;

			this->resetKillValidations();
		}

		auto getArea(const SVector3& vec) const -> const Area* {
			for (auto const& area : this->areas) {
				auto lowZ = area.From.z < area.To.z ? area.From.z : area.To.z;
				if (vec.z < lowZ) continue;
				auto highZ = area.From.z > area.To.z ? area.From.z : area.To.z;
				if (vec.z > highZ) continue;
				auto lowY = area.From.y < area.To.y ? area.From.y : area.To.y;
				if (vec.y < lowY) continue;
				auto highY = area.From.y > area.To.y ? area.From.y : area.To.y;
				if (vec.y > highY) continue;
				auto lowX = area.From.x < area.To.x ? area.From.x : area.To.x;
				if (vec.x < lowX) continue;
				auto highX = area.From.x > area.To.x ? area.From.x : area.To.x;
				if (vec.x > highX) continue;
				return &area;
			}
			return nullptr;
		}

		auto getActorDataByRepoId(const ZRepositoryID& repoId) -> ActorData* {
			auto it = this->actorDataRepoIdMap.find(repoId);
			if (it != this->actorDataRepoIdMap.end())
				return &this->actorData[it->second];
			return nullptr;
		}

		auto getActorDataByRepoId(const ZRepositoryID& repoId) const -> const ActorData* {
			return const_cast<State*>(this)->getActorDataByRepoId(repoId);
		}

		auto getTargetKillValidation(eTargetID target) const -> KillConfirmation {
			//if (hasLoadedGame) return KillConfirmation(target, eKillValidationType::Unknown);
			for (auto& kc : killValidations) {
				if (kc.target == target)
					return kc;
			}
			return KillConfirmation(target, eKillValidationType::Incomplete);
		}

		auto getKillConfirmation(size_t idx) -> KillConfirmation& {
			if (killValidations.size() < spin.getConditions().size())
				this->reset();
			if (idx > killValidations.size()) throw std::exception("Invalid kill confirmation index.");
			return killValidations[idx];
		}

		auto getLastDisguiseChangeAtTimestamp(float timestamp) const -> const DisguiseChange* {
			for (auto i = disguiseChanges.size(); i > 0; --i) {
				if (disguiseChanges[i - 1].timestamp < timestamp)
					return &disguiseChanges[i - 1];
			}
			return nullptr;
		}

		auto getSetpieceByName(std::string_view name) const -> const KillSetpieceEvent* {
			for (auto i = killSetpieceEvents.size(); i > 0; --i) {
				if (killSetpieceEvents[i - 1].name == name)
					return &killSetpieceEvents[i - 1];
			}
			return nullptr;
		}

		auto getSetpieceEventAtTimestamp(double timestamp, double margin = 0.1) const -> const KillSetpieceEvent* {
			for (auto i = killSetpieceEvents.size(); i > 0; --i) {
				if (std::abs(killSetpieceEvents[i - 1].timestamp - timestamp) < margin)
					return &killSetpieceEvents[i - 1];
			}
			return nullptr;
		}

		auto getLevelSetupEventByEvent(std::string_view name) const -> const LevelSetupEvent* {
			for (auto i = levelSetupEvents.size(); i > 0; --i) {
				if (levelSetupEvents[i - 1].event == name)
					return &levelSetupEvents[i - 1];
			}
			return nullptr;
		}

		auto getLevelSetupEventAtTimestamp(double timestamp, double margin = 0.1) const -> const LevelSetupEvent* {
			for (auto i = levelSetupEvents.size(); i > 0; --i) {
				if (std::abs(levelSetupEvents[i - 1].timestamp - timestamp) < margin)
					return &levelSetupEvents[i - 1];
			}
			return nullptr;
		}

		auto getLastDisguiseChange() const -> const DisguiseChange* {
			return disguiseChanges.empty() ? nullptr : &disguiseChanges.back();
		}

		auto getTimeElapsed() const -> std::chrono::seconds {
			if (!this->isFinished && this->isPlaying) {
				return std::chrono::duration_cast<std::chrono::seconds>(std::chrono::steady_clock().now() - timeStarted);
			}
			return this->isFinished ? this->timeElapsed : std::chrono::seconds::zero();
		}

		auto voidSA() {
			if (this->isFinished) return;
			this->isSA = false;
		}

		auto playerSelectMission() {
			this->isPlaying = false;
			this->isFinished = false;
		}

		auto playerStart() {
			this->reset();

			if (!this->isPlaying) {
				this->isPlaying = true;
				this->timeStarted = std::chrono::steady_clock().now();
				this->isFinished = false;
			}

			this->exitIGT = 0;
			this->isSA = true;
			this->isCaughtOnCams = false;
			this->isCamsDestroyed = false;
			this->hasLoadedGame = false;
		}

		auto playerCutsceneEnd(double igt) {
			this->startIGT = igt;
			this->isPlaying = true;
		}

		auto playerLoad() {
			this->isPlaying = true;
			this->hasLoadedGame = true;
			this->reset();
		}

		auto playerExit(double timestamp = 0) {
			this->timeElapsed = this->getTimeElapsed();
			this->isPlaying = false;
			this->isFinished = true;
			if (this->spottedNotKilled.size() > 0)
				this->isSA = false;
			this->exitIGT = timestamp - this->startIGT;
			this->isSA = this->isSA && !this->isCaughtOnCams && !this->hasLoadedGame;
			this->killed.clear();
			this->spottedNotKilled.clear();
		}

		auto resetKillValidations() -> void {
			killValidations.resize(spin.getConditions().size());

			for (auto& kv : killValidations)
				kv = KillConfirmation {};
		}

		static auto OnRulesetSelect(eRouletteRuleset ruleset) -> void;

		static auto OnMissionSelect(eMission, bool isAuto = true) -> void;

		static auto OnRulesetCustomised() -> void;
	};
}
