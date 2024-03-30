// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"

BOOL APIENTRY DllMain(HMODULE hModule,
    DWORD  ul_reason_for_call,
    LPVOID lpReserved
)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

extern "C" __declspec(dllexport) void InputChar(int c)
{
    INPUT input{};
    input.type = INPUT_KEYBOARD;
    input.ki.dwFlags = 0;
    input.ki.time = 0;
    input.ki.dwExtraInfo = 0;

    input.ki.wVk = c;
    SendInput(1, &input, sizeof(INPUT));
}

extern "C" __declspec(dllexport) void InputReturn()
{
    INPUT input{};
    input.type = INPUT_KEYBOARD;
    input.ki.dwFlags = 0;
    input.ki.time = 0;
    input.ki.dwExtraInfo = 0;

    input.ki.wVk = VK_RETURN;
    SendInput(1, &input, sizeof(INPUT));
}

extern "C" __declspec(dllexport) void InputLeft()
{
    INPUT input{};
    input.type = INPUT_KEYBOARD;
    input.ki.dwFlags = 0;
    input.ki.time = 0;
    input.ki.dwExtraInfo = 0;

    input.ki.wVk = VK_LEFT;
    SendInput(1, &input, sizeof(INPUT));
}
