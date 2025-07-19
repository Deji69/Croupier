#pragma once
#include <string_view>
#include <EngineFunction.h>
#include <Glacier/Hash.h>
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
#include <memory>
#include <optional>
#include "ProcessUtils.h"

inline auto GetPropertyIDs(ZEntityRef s_Entity) -> std::vector<std::pair<uint32, std::string_view>> {
    if (!s_Entity || !*Globals::MemoryManager)
		return {};

    const auto s_Type = s_Entity->GetType();

    if (!s_Type) return {};

	std::vector<std::pair<uint32, std::string_view>> vec;

    for (uint32_t i = 0; i < s_Type->m_pProperties01->size(); ++i) {
        const ZEntityProperty* s_Property = &s_Type->m_pProperties01->operator[](i);
		const auto* s_PropertyInfo = s_Property->m_pType->getPropertyInfo();
		const std::string_view s_TypeName = s_PropertyInfo->m_pType->typeInfo()->m_pTypeName;
		vec.emplace_back(s_Property->m_nPropertyId, s_TypeName);
    }

    return vec;
}

// Workaround for ZHM method not handling non default constructible types
template<typename T>
inline auto GetValueProperty(ZEntityRef s_Entity, const uint32_t nPropertyID) -> std::unique_ptr<T, std::function<void(T*)>> {
    if (!s_Entity || !*Globals::MemoryManager)
        return nullptr;

    const auto s_Type = s_Entity->GetType();

    if (!s_Type)
        return nullptr;

    for (uint32_t i = 0; i < s_Type->m_pProperties01->size(); ++i) {
        const ZEntityProperty* s_Property = &s_Type->m_pProperties01->operator[](i);

        if (s_Property->m_nPropertyId != nPropertyID)
            continue;

        const auto* s_PropertyInfo = s_Property->m_pType->getPropertyInfo();

		if (!s_PropertyInfo || !s_PropertyInfo->m_pType)
			continue;

        const auto s_PropertyAddress = reinterpret_cast<uintptr_t>(s_Entity.m_pEntity) + s_Property->m_nOffset;

        const uint16_t s_TypeSize = s_PropertyInfo->m_pType->typeInfo()->m_nTypeSize;
        const uint16_t s_TypeAlignment = s_PropertyInfo->m_pType->typeInfo()->m_nTypeAlignment;
		const std::string_view s_TypeName = s_PropertyInfo->m_pType->typeInfo()->m_pTypeName;

        auto* s_Data = (*Globals::MemoryManager)->m_pNormalAllocator->AllocateAligned(s_TypeSize, s_TypeAlignment);
		if (!s_Data) break;

        if (s_PropertyInfo->m_nFlags & EPropertyInfoFlags::E_HAS_GETTER_SETTER) {
            s_PropertyInfo->get(reinterpret_cast<void*>(s_PropertyAddress), s_Data, s_PropertyInfo->m_nOffset);
        }
        else {
            s_PropertyInfo->m_pType->typeInfo()->m_pTypeFunctions->copyConstruct(
                s_Data, reinterpret_cast<void*>(s_PropertyAddress)
            );
        }

		return std::unique_ptr<T, std::function<void(T*)>>(reinterpret_cast<T*>(s_Data), [s_Data](T*) {
			(*Globals::MemoryManager)->m_pNormalAllocator->Free(s_Data);
		});
    }

    return nullptr;
}

template<typename T>
inline auto GetValueProperty(ZEntityRef s_Entity, const ZString& p_PropertyName) -> std::unique_ptr<T, std::function<void(T*)>> {
    return GetValueProperty<T>(s_Entity, Hash::Crc32(p_PropertyName.c_str(), p_PropertyName.size()));
}

template<typename T>
inline auto QueryAnyParent(ZEntityRef entity) -> T* {
	if (!entity) return nullptr;
	auto interf = entity.QueryInterface<T>();
	if (interf) return interf;
	return QueryAnyParent<T>(entity.GetLogicalParent());
}

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
	uint32 unk;
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

// 0x0000000144540BF0 (Size: 0x4C0)
class ZHM5ItemCCWeapon : public ZHM5Item, public IItemWeapon
{
public:
	ECCWeaponAnimSet m_eAnimSetFrontSide; // 0x448
	ECCWeaponAnimSet m_eAnimSetBack; // 0x44C
	PAD(0x18); // TArray<TEntityRef<ZCCEffectSetEntity>> m_aEffectSetsFrontSide; // 0x450
	PAD(0x18); // TArray<TEntityRef<ZCCEffectSetEntity>> m_aEffectSetsBack; // 0x468
	EActorSoundDefs m_eDeathSpeakFront; // 0x480
	EActorSoundDefs m_eDeathSpeakBack; // 0x484
	EActorSoundDefs m_eReactionSpeak; // 0x488
	EDeathType m_eDeathTypeFront; // 0x48C
	EDeathType m_eDeathTypeBack; // 0x490
	int32 m_nLifeSpan; // 0x494
	bool m_bCountsAsFiberWire; // 0x498
	TEntityRef<ZValueBool> m_rCCWeaponBroken; // 0x4A0
};

class ZKeywordEntity : public ZEntityImpl
{
public:
	ZString m_sKeyword; // 0x18
	ZEntityRef m_rHolder; // 0x28
	TArray<ZEntityRef> m_aHolders; // 0x30
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

class ZActorOutfitListener : public ZEntityImpl
{
public:
	TEntityRef<ZActor> m_rActor; // 0x18
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

