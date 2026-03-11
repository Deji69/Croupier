#pragma once
#include "ProcessUtils.h"
#include <Common.h>
#include <cstdarg>
#include <cstdint>
#include <EngineFunction.h>
#include <fmt/format.h>
#include <functional>
#include <Glacier/Enums.h>
#include <Glacier/IComponentInterface.h>
#include <Glacier/Reflection.h>
#include <Glacier/TArray.h>
#include <Glacier/ZActor.h>
#include <Glacier/ZEntity.h>
#include <Glacier/ZHitman5.h>
#include <Glacier/ZHM5BaseCharacter.h>
#include <Glacier/ZHM5GridManager.h>
#include <Glacier/ZItem.h>
#include <GLacier/ZMath.h>
#include <Glacier/ZPrimitives.h>
#include <Glacier/ZResource.h>
#include <Glacier/ZResourceID.h>
#include <Glacier/ZSpatialEntity.h>
#include <Glacier/ZString.h>
#include <Globals.h>
#include <Logging.h>
#include <memory>
#include <string_view>
#include <type_traits>
#include <utility>
#include <vector>
#include <Windows.h>
#include <Glacier/ZAction.h>

inline auto GetPropertyIDs(ZEntityRef s_Entity) -> std::vector<std::pair<uint32, std::string_view>> {
	if (!s_Entity || !*Globals::MemoryManager)
		return {};

	const auto s_Type = s_Entity->GetType();

	if (!s_Type) return {};

	std::vector<std::pair<uint32, std::string_view>> vec;

	for (uint32_t i = 0; i < s_Type->m_pPropertyData->size(); ++i) {
		const SPropertyData* s_Property = &s_Type->m_pPropertyData->operator[](i);
		const auto* s_PropertyInfo = s_Property->m_pPropertyInfo;
		const std::string_view s_TypeName = s_PropertyInfo->m_Type->GetTypeInfo()->pszTypeName;
		vec.emplace_back(s_Property->m_nPropertyID, s_TypeName);
	}

	return vec;
}

// Workaround for ZHM method not handling non default constructible types
template<typename T>
inline auto GetValueProperty(ZEntityRef s_Entity, const uint32_t nPropertyID) -> std::unique_ptr<T, std::function<void(T*)>> {
	if (!s_Entity || !*Globals::MemoryManager)
		return nullptr;

	const auto s_Type = s_Entity->GetType();

	if (!s_Type || !s_Type->m_pPropertyData)
		return nullptr;

	for (uint32_t i = 0; i < s_Type->m_pPropertyData->size(); ++i) {
		const SPropertyData* s_Property = &s_Type->m_pPropertyData->operator[](i);

		if (s_Property->m_nPropertyID != nPropertyID)
			continue;

		const auto* s_PropertyInfo = s_Property->m_pPropertyInfo;

		if (!s_PropertyInfo || !s_PropertyInfo->m_Type)
			continue;

		const auto s_PropertyAddress = reinterpret_cast<uintptr_t>(s_Entity.m_pObj) + s_Property->m_nPropertyOffset;

		const uint16_t s_TypeSize = s_PropertyInfo->m_Type->GetTypeInfo()->m_nTypeSize;
		const uint16_t s_TypeAlignment = s_PropertyInfo->m_Type->GetTypeInfo()->m_nTypeAlignment;
		const std::string_view s_TypeName = s_PropertyInfo->m_Type->GetTypeInfo()->pszTypeName;

		auto* s_Data = (*Globals::MemoryManager)->m_pNormalAllocator->AllocateAligned(s_TypeSize, s_TypeAlignment);
		if (!s_Data) break;

		if (s_PropertyInfo->m_Flags & EPropertyInfoFlags::E_HAS_GETTER_SETTER) {
			s_PropertyInfo->m_PropetyGetter(reinterpret_cast<void*>(s_PropertyAddress), s_Data, s_Property->m_nPropertyOffset);
		}
		else {
			s_PropertyInfo->m_Type->GetTypeInfo()->m_pTypeFunctions->placementCopyConstruct(
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
inline auto GetClosestEntityWithProperty(ZEntityRef s_Entity, const uint32_t nPropertyID) -> ZEntityRef {
	if (!s_Entity)
		return nullptr;

	for (auto entity = s_Entity; entity; entity = entity.GetLogicalParent()) {
		const auto s_Type = entity->GetType();

		if (!s_Type) continue;
		if (!s_Type->m_pPropertyData) continue;

		for (uint32_t i = 0; i < s_Type->m_pPropertyData->size(); ++i) {
			const auto* s_Property = &s_Type->m_pPropertyData->operator[](i);

			if (s_Property->m_nPropertyID != nPropertyID)
				continue;

			const auto* s_PropertyInfo = s_Property->GetPropertyInfo();

			if (!s_PropertyInfo || !s_PropertyInfo->m_propertyInfo.m_Type)
				continue;

			return entity;
		}
	}

	return nullptr;
}

template<typename T>
inline auto GetValueProperty(ZEntityRef s_Entity, const ZString& p_PropertyName) -> std::unique_ptr<T, std::function<void(T*)>> {
	return GetValueProperty<T>(s_Entity, Hash::Crc32(p_PropertyName.c_str(), p_PropertyName.size()));
}

template<typename T>
inline auto GetClosestEntityWithProperty(ZEntityRef s_Entity, const ZString& p_PropertyName) -> ZEntityRef {
	return GetClosestEntityWithProperty<T>(s_Entity, Hash::Crc32(p_PropertyName.c_str(), p_PropertyName.size()));
}

template<typename T>
inline auto GetValuePropertyFromTree(ZEntityRef s_Entity, const uint32_t nPropertyID) -> std::unique_ptr<T, std::function<void(T*)>> {
	if (!s_Entity) return nullptr;
	auto ent = GetClosestEntityWithProperty<T>(s_Entity, nPropertyID);
	if (!ent) return nullptr;
	auto res = GetValueProperty<T>(ent, nPropertyID);
	if (res) return res;
	return nullptr;
}

template<typename T>
inline auto GetValuePropertyFromTree(ZEntityRef s_Entity, const ZString& p_PropertyName) -> std::unique_ptr<T, std::function<void(T*)>> {
	return GetValuePropertyFromTree<T>(s_Entity, Hash::Crc32(p_PropertyName.c_str(), p_PropertyName.size()));
}

template<typename T>
inline auto QueryAnyParent(ZEntityRef entity) -> T* {
	if (!entity) return nullptr;
	auto interf = entity.QueryInterface<T>();
	if (interf) return interf;
	return QueryAnyParent<T>(entity.GetLogicalParent());
}

//class IItemWeapon : public IComponentInterface
//{
//public:
//	virtual ~IItemWeapon() = 0;
//};

//class IFirearm : public IComponentInterface
//{
//public:
//	virtual ~IFirearm() = 0;
//};

class ZPFAreaRef
{
public:
	auto GetRegionMask() const -> ERegionMask;

	void* m_handleImpl;
	void* m_pSpace;
};

#pragma pack(push, 4)

//class ZPFLocation
//{
//public:
//	SVector3 m_pos;
//	ZPFAreaRef m_area;
//	bool m_bMapped;
//};

#pragma pack(pop)

class CroupierZHM5GridManager : public IComponentInterface
{
public:
	PAD(0x40);
	ZPFLocation m_HitmanPFLocation; // 0x40
	ZGridNodeRef m_HitmanNode; //0x68
};

class ZGuideAgility :
	public ZBoundedEntity // Offset 0x0
{
public:
	bool m_bIllegal; // 0x101
	TArray<TEntityRef<ZOutfitProfessionEntity>> m_bLegalProfessions; // 0x108
	bool m_bStartEnabled; // 0x120
};

class ZGuideLadder :
	public ZGuideAgility // Offset 0x0
{
public:
	int32 m_nNumOfRungs; // 0x130
	ZResourcePtr m_pHelper; // 0x134
};

class ZGuideDrainPipe :
	public ZGuideAgility // Offset 0x0
{
public:
	uint32 m_nNumSteps; // 0x130
	bool m_bTopEnabled; // 0x134
	bool m_bBottomEnabled; // 0x135
	TEntityRef<ZBoxVolumeEntity> m_rCombatArea; // 0x138
};

class ZGuideWindow :
	public ZGuideAgility // Offset 0x0
{
public:
	ZResourcePtr m_pHelper; // 0x130
	float32 m_fWidth; // 0x138
	float32 m_fDepth; // 0x13C
	float32 m_fHeight; // 0x140
	TEntityRef<ZSpatialEntity> m_WindowFrame; // 0x148
	float32 m_fOffsetFromSide; // 0x158
	bool m_bAccessibleFromRight; // 0x15C
	bool m_bAccessibleFromLeft; // 0x15D
	bool m_bClosed; // 0x15E
	bool m_bPushVictim; // 0x15F
	bool m_bPacifyPushVictim; // 0x160
	TResourcePtr<void> m_pTextListResource; // 0x164
	ZString m_sEnterTextID; // 0x170
};

class ZGuideLedge :
	public ZGuideAgility // Offset 0x0
{
public:
	bool m_bWalkable; // 0x130
	bool m_bMountableFromTop; // 0x131
	bool m_bMountableFromBottom; // 0x132
	bool m_bMountDismountLeft; // 0x133
	bool m_bMountDismountRight; // 0x134
	bool m_bCanStandOnLedge; // 0x135
	bool m_bCanMountBothSides; // 0x136
	bool m_bDeactivateOnWalk; // 0x137
	bool m_bCanHang; // 0x138
	bool m_bAllowDropDown; // 0x139
	bool m_bAllowJumpToLedgeLeft; // 0x13A
	bool m_bAllowJumpToLedgeRight; // 0x13B
	ELedgeDismountBehavior m_eDismountUpBehavior; // 0x13C
	ELedgeDismountBehavior m_eDismountDownBehavior; // 0x140
	ELedgeDismountDirection m_eDismountDownDirection; // 0x144
	bool m_bKickNPCEnabled; // 0x148
	bool m_bKickNPCIsAccident; // 0x149
	bool m_bGrabNPCEnabled; // 0x14A
	bool m_bPacifyKickedNPC; // 0x14B
	bool m_bBodyDumpEnabled; // 0x14C
	bool m_bDumpedBodyHidden; // 0x14D
	bool m_bAnchorEnabled; // 0x14E
	SVector2 m_vSize; // 0x150
	TResourcePtr<void> m_pTextListResource; // 0x158
	ZString m_sEnterTextID; // 0x160
	bool m_bBlockVisionAmbient; // 0x170
	bool m_bBlockVisionAlerted; // 0x171
	TEntityRef<ZBoxVolumeEntity> m_rCombatArea; // 0x178
};

class SHitInfo
{
public:
	ZEntityRef m_rHitEntity; // 0x0
	ZEntityRef m_rWeapon; // 0x8
	ZEntityRef m_rWeaponOwner; // 0x10
	ZEntityRef m_rSetpiece; // 0x18
	ZEntityRef m_rHitCrowdActor; // 0x20
	char m_pHitBody[0x8]; // 0x28
	uint32 m_nHitBoneIndex; // 0x30
	float32 m_fBloodSplatModifier; // 0x34
	float4 m_vHitPos; // 0x40
	float4 m_vHitNormal; // 0x50
	float4 m_vHitDistance; // 0x60
	char m_Explosion[0x60]; // 0x70
	char m_coliIn[0x20]; // 0xD0
	char m_coliOut[0x70]; // 0xF0
	char m_Projectile[0x90]; // 0x160
	uint32 m_nDeathType; // 0x1F0
	uint32 m_nDeathContext; // 0x1F4
	float32 m_fContactForce; // 0x1F8
};

class IItemContainer : public IComponentInterface {
public:
};

class IBoolCondition : public IComponentInterface {
public:
};

class ZItemStorageEntity :
	public ZEntityImpl, // Offset 0x0 
	public IItemContainer, // Offset 0x18 
	public IBoolCondition, // Offset 0x20 
	public IBoolConditionListener, // Offset 0x28 
	public IHM5ActionEntityListener, // Offset 0x30 
	public IItemOwner, // Offset 0x38 
	public ISavableEntity // Offset 0x40
{
public:
	TEntityRef<ZSpatialEntity> m_rContainerGeom; // 0x48
	TEntityRef<ZItemStorageEntity> m_rStorage; // 0x58
	TEntityRef<ZValueBool> m_rContainsItem; // 0x68
	TEntityRef<ZValueBool> m_rIsBroken; // 0x78
};

class ZInteractionCondition :
	public ZEntityImpl, // Offset 0x0 
	public IBoolCondition, // Offset 0x18 
	public IBoolConditionListener // Offset 0x20
{
public:
	bool m_bConditionValue; // 0x28
	TEntityRef<IBoolCondition> m_rEnabled; // 0x30
	EHM5GameInputFlag m_eInputAction; // 0x40
	EActionType m_eActionType; // 0x44
	EBaseMovementType m_eMovementType; // 0x48
	TEntityRef<void> m_rSubaction; // 0x50
	ZEntityRef m_rObject; // 0x60
	ZInteractionData_EFilterResult m_eFilterResult; // 0x68
	EBoolCheckType m_eIllegalityEval; // 0x6C
	EBoolCheckType m_eIllegalItemEval; // 0x70
	EBoolCheckType m_eInputIsApplied; // 0x74
};

/*class ZHM5ItemWeapon :
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
};*/

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

