#include "ProcessUtils.h"
#include "ZHMUtils.h"
#include <cstdarg>
#include <cstdint>
#include <Glacier/Enums.h>
#include <Glacier/ZMath.h>
#include <Glacier/ZPrimitives.h>
#include <Logging.h>
#include <memory>
#include <Windows.h>

template <class T>
class PatternEngineFunction;

HMODULE ZHMExtension::Module;
uintptr_t ZHMExtension::ModuleBase;
uint32_t ZHMExtension::SizeOfCode;
uint32_t ZHMExtension::ImageSize;
ZRoomManagerCreator* ZHMExtension::RoomManagerCreator = nullptr;
static std::unique_ptr<PatternEngineFunction<int16(ZRoomManager* th, const float4 vPointWS)>> ZRoomManager_CheckPointInRoom;
static std::unique_ptr<PatternEngineFunction<ERegionMask(const ZPFAreaRef* area)>> ZPFAreaRef_GetAreaUsageFlags;

auto ZHMExtension::Init() -> void {
	Module = GetModuleHandleA(nullptr);
	ModuleBase = reinterpret_cast<uintptr_t>(Module) + Util::ProcessUtils::GetBaseOfCode(Module);
	SizeOfCode = Util::ProcessUtils::GetSizeOfCode(Module);
	ImageSize = Util::ProcessUtils::GetSizeOfImage(Module);
	RoomManagerCreator = PatternGlobalRelative<ZRoomManagerCreator*>(
		"RoomManagerCreator",
		"\x48\x89\x05\x00\x00\x00\x00\xE8\x00\x00\x00\x00\x45\x33\xC9\x4C\x8D\x45\x20\xBA\x00\x00\x00\x00\x48\x8B\x48\x10\x48\x8B\x01\xFF\x50\x38\x48\x85\xC0",
		"xxx????x????xxxxxxxx????xxxxxxxxxxxxx",
		3
	);
	if (!RoomManagerCreator) {
		Logger::Error("[Croupier] Could not locate address for function 'RoomManagerCreator'. Bingo room entry checks will not work.");
	}
	ZRoomManager_CheckPointInRoom = std::make_unique<PatternEngineFunction<int16(ZRoomManager * th, const float4 vPointWS)>>(
		"ZRoomManager_CheckPointInRoom",
		"\x48\x89\x5C\x24\x08\x48\x89\x6C\x24\x10\x48\x89\x74\x24\x18\x48\x89\x7C\x24\x20\x41\x56\x48\x83\xEC\x00\x48\x8B\xEA\x48\x8B\xF1\x33\xDB",
		"xxxxxxxxxxxxxxxxxxxxxxxxx?xxxxxxxx"
	);
	if (!ZRoomManager_CheckPointInRoom->IsFound()) {
		Logger::Error("[Croupier] Could not locate address for function 'ZRoomManager_CheckPointInRoom'. Bingo room entry checks will not work.");
	}

	ZPFAreaRef_GetAreaUsageFlags = std::make_unique<PatternEngineFunction<ERegionMask(const ZPFAreaRef* area)>>(
		"ZPFAreaRef_GetAreaUsageFlags",
		"\x48\x89\x5C\x24\x08\x57\x48\x83\xEC\x00\x48\x8B\x1D\x00\x00\x00\x00\x48\x8B\xF9\x48\x8B\x5B\x70\x48\x85\xDB\x74\x00\x48\x8B\xCB\xFF\x15\x00\x00\x00\x00\x48\x8B\x07\x48\x85\xC0\x74\x00\x48\x8B\x08",
		"xxxxxxxxx?xxx????xxxxxxxxxxx?xxxxx????xxxxxxx?xxx"
	);
	if (!ZPFAreaRef_GetAreaUsageFlags->IsFound()) {
		Logger::Error("[Croupier] Could not locate address for function 'ZPFAreaRef_GetAreaUsageFlags'. Bingo staircase checks will not work.");
	}
}

auto ZRoomManagerCreator::GetRoomID(const float4 pos) -> int16 {
	if (!ZHMExtension::RoomManagerCreator) return -1;
	auto roomManager = ZHMExtension::RoomManagerCreator->m_pRoomManager;
	if (!roomManager) return -1;
	return roomManager->GetRoomID(pos);
}

auto ZRoomManager::GetRoomID(const float4 vPointWS) -> int16 {
	return ZRoomManager_CheckPointInRoom->IsFound()
		? ZRoomManager_CheckPointInRoom->Call(this, vPointWS)
		: -1;
}

auto ZPFAreaRef::GetRegionMask() const -> ERegionMask {
	return ZPFAreaRef_GetAreaUsageFlags->IsFound()
		? ZPFAreaRef_GetAreaUsageFlags->Call(this)
		: ERegionMask::eRM_None;
}
