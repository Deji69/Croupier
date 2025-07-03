#include "ZHMUtils.h"
#include <memory>

template <class T>
class PatternEngineFunction;

HMODULE ZHMExtension::Module;
uintptr_t ZHMExtension::ModuleBase;
uint32_t ZHMExtension::SizeOfCode;
uint32_t ZHMExtension::ImageSize;
ZRoomManagerCreator* ZHMExtension::RoomManagerCreator = nullptr;
static std::unique_ptr<PatternEngineFunction<int16(ZRoomManager* th, const float4 vPointWS)>> ZRoomManager_CheckPointInRoom;

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
		Logger::Error("[Croupier] Could not locate address for function 'ZRoomManager_CheckPointInRoom'. Bingo room entry checks will not work.");
	}
	ZRoomManager_CheckPointInRoom = std::make_unique<PatternEngineFunction<int16(ZRoomManager * th, const float4 vPointWS)>>(
		"ZRoomManager_CheckPointInRoom",
		"\x48\x89\x5C\x24\x08\x48\x89\x6C\x24\x10\x48\x89\x74\x24\x18\x48\x89\x7C\x24\x20\x41\x56\x48\x83\xEC\x20\x48\x8B\xEA\x48\x8B\xF1\x33\xDB\x49\xBE\xB7\x6D\xDB\xB6\x6D\xDB\xB6\x6D\x0F\x1F\x40\x00",
		"xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
	);
	if (!ZRoomManager_CheckPointInRoom->IsFound()) {
		Logger::Error("[Croupier] Could not locate address for function 'ZRoomManager_CheckPointInRoom'. Bingo room entry checks will not work.");
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

