#pragma once
#include <Hooks.h>
#include <IPluginInterface.h>
#include <EngineFunction.h>
#include <Glacier/TArray.h>
#include <Glacier/ZGameUIManager.h>
#include <Glacier/ZInput.h>
#include <Glacier/ZObject.h>
#include <Glacier/ZString.h>
#include <Logging.h>
#include <ResourceLib_HM3.h>
#include "ProcessUtils.h"

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

