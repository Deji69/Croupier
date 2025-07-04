#pragma once
#include <EngineFunction.h>
#include <Glacier/TArray.h>
#include <Glacier/ZGameUIManager.h>
#include <Glacier/ZHitman5.h>
#include <Glacier/ZInput.h>
#include <Glacier/ZItem.h>
#include <Glacier/ZObject.h>
#include <Glacier/ZResource.h>
#include <Glacier/ZSpatialEntity.h>
#include <Glacier/ZString.h>
#include <Hooks.h>
#include <IPluginInterface.h>
#include <Logging.h>
#include "ProcessUtils.h"

class IItemWeapon : public IComponentInterface
{
public:
	virtual ~IItemWeapon() = 0;
};

class IFirearm : public IComponentInterface
{
public:
	virtual ~IFirearm() = 0;
};

class ZHM5ItemWeapon :
	public ZHM5Item, // Offset 0x0 
	public IItemWeapon, // Offset 0x440 
	public IFirearm // Offset 0x448 
{
public:
	PAD(0x4);
	eWeaponType m_WeaponType; // 0x464
	ZRuntimeResourceID m_ridClipTemplate; // 0x468
	EWeaponAnimationCategory m_eAnimationCategory; // 0x470
	ZEntityRef m_MuzzleExit; // 0x478
	ZEntityRef m_CartridgeEject; // 0x480
	float32 m_fCartridgeEjectForceMultiplier; // 0x488
	TEntityRef<void> m_MuzzleFlashEffect; // 0x490
	TEntityRef<void> m_MuzzleSmokeEffect; // 0x4A0
	TEntityRef<void> m_MuzzleFlashEffectGroup; // 0x4B0
	TEntityRef<void> m_MuzzleSmokeEffectGroup; // 0x4C0
	TEntityRef<void> m_SoundSetup; // 0x4D0
	TEntityRef<void> m_AudioSetup; // 0x4E0
	TEntityRef<void> m_LeftHandPos; // 0x4F0
	ZEntityRef m_AmmoProperties; // 0x500
	bool m_bConnectsToTarget; // 0x508
	float32 m_fMuzzleEnergyMultiplier; // 0x50C
	bool m_bScopedWeapon; // 0x510
	ZEntityRef m_ScopePosition; // 0x518
	ZEntityRef m_ScopeCrossHair; // 0x520
	ZEntityRef m_rSpecialImpactAct; // 0x528
	ZEntityRef m_rSuperSpecialTriggerEffect; // 0x530
	PAD(0x398);
};

class ZVIPControllerEntity : public ZEntityImpl
{
public:
	TEntityRef<ZActor> m_rVIP; // 0x18
	EActorFaction m_eFaction; // 0x28
	TArray<TEntityRef<ZActor>> m_aPreferredEntourage; // 0x30
	//TArray<TEntityRef<ZVIPDestinationEntity>> m_aDestinations; // 0x48
	PAD(0x18);
	bool m_usePreferredNextNodes; // 0x60
	bool m_bVIPHandlesCuriousInvestigations; // 0x61
	bool m_bAllowSecondaryGuardsToEvacuate; // 0x62
};

class ZExplodingPropCounter : public ZEntityImpl
{
public:
	EGSExplodingPropType m_eExplodingPropType; // 0x18
};

class ISoundGateController {
public:
	virtual ~ISoundGateController() = 0;
};

class IHM5Door : public ISoundGateController {
public:
	enum class EInitialState {
		IS_CLOSED = 0,
		IS_OPEN = 1,
		IS_OPEN_IN = 2,
		IS_OPEN_OUT = 3,
	};
	enum class EOpenDir {
		OD_AWAY = 0,
		OD_TOWARS = 1,
		OD_IN = 2,
		OD_OUT = 3,
	};
	enum class EOpenMode {
		OM_TWO_WAY = 0,
		OM_OPEN_POS_SIDE_ONLY = 1,
		OM_OPEN_NEG_SIDE_ONLY = 2,
		OM_DISABLED = 3,
	};

	virtual ~IHM5Door() = 0;
};

class ZHM5SingleDoor2 :
	public ZBoundedEntity, // Offset 0x0 
	public IHM5Door // Offset 0xB8
{
public:
	PAD(0x520);
};

class ZHM5DoubleDoor2 : public ZHM5SingleDoor2
{
public:
	TEntityRef<ZSpatialEntity> m_rDoorFrame2; // 0x5E0
};

struct SRoomInfoHeader {
	PAD(0xD0);
};

class ZRoomManager {
public:
	int16 GetRoomID(const float4 vPointWS);

    PAD(0x6D0);
    TArray<SRoomInfoHeader> m_RoomHeaders;
};

class ZRoomManagerCreator : public IComponentInterface {
public:
	static auto GetRoomID(const float4 pos) -> int16;

	ZRoomManager* m_pRoomManager;
};

class ZHMExtension {
public:
	static HMODULE Module;
	static uintptr_t ModuleBase;
	static uint32_t SizeOfCode;
	static uint32_t ImageSize;
	static ZRoomManagerCreator* RoomManagerCreator;

	static auto Init() -> void;
};

template<typename T>
class PatternEngineFunction;

template<typename ReturnType, typename ...Args>
class PatternEngineFunction<ReturnType(Args...)> final : public EngineFunction<ReturnType(Args...)> {
public:
    PatternEngineFunction(const char* p_FunctionName, const char* p_Pattern, const char* p_Mask) :
        EngineFunction<ReturnType(Args...)>(GetTarget(p_Pattern, p_Mask))
    {
    }

	auto IsFound() const {
		return this->m_Address != nullptr;
	}

private:
    void* GetTarget(const char* p_Pattern, const char* p_Mask) const {
        const auto* s_Pattern = reinterpret_cast<const uint8_t*>(p_Pattern);
        return reinterpret_cast<void*>(Util::ProcessUtils::SearchPattern(ZHMExtension::ModuleBase, ZHMExtension::SizeOfCode, s_Pattern, p_Mask));
    }
};

template <class T>
T PatternGlobalRelative(const char* p_GlobalName, const char* p_Pattern, const char* p_Mask, ptrdiff_t p_Offset) {
    static_assert(std::is_pointer<T>::value, "Global type is not a pointer type.");

    const auto* s_Pattern = reinterpret_cast<const uint8_t*>(p_Pattern);
    auto s_Target = Util::ProcessUtils::SearchPattern(ZHMExtension::ModuleBase, ZHMExtension::SizeOfCode, s_Pattern, p_Mask);

    if (s_Target == 0) {
        Logger::Error("Could not find address for global '{}'. This probably means that the game was updated and the SDK requires changes.", p_GlobalName);
        return nullptr;
    }

    uintptr_t s_RelAddrPtr = s_Target + p_Offset;
    int32_t s_RelAddr = *reinterpret_cast<int32_t*>(s_RelAddrPtr);
    uintptr_t s_FinalAddr = s_RelAddrPtr + s_RelAddr + sizeof(int32_t);

    Logger::Debug("Successfully located global '{}' at address {}.", p_GlobalName, fmt::ptr(reinterpret_cast<void*>(s_FinalAddr)));

    return reinterpret_cast<T>(s_FinalAddr);
}

