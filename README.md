# 🤬 Rude Discord Bot

![C#](https://img.shields.io/badge/C%23-.NET%208-512BD4?logo=dotnet&logoColor=white)
![Discord.Net](https://img.shields.io/badge/Discord.Net-3.18-5865F2?logo=discord&logoColor=white)
![Ollama](https://img.shields.io/badge/LLM-Ollama%20(local)-black)
![Model](https://img.shields.io/badge/Model-Llama%203.1-orange)

A Discord bot written in **C# / .NET 8** that answers questions with a sarcastic, roast-style personality, powered by a **Llama 3.1 model running locally through Ollama**.

No cloud LLM API, no per-message cost: every reply is generated on your own machine.

---

## ✨ Features

- **`!Ola <question>`** sends your prompt to the local Llama 3.1 model and replies in character
- **Placeholder, then edit:** the bot replies *"Thu ruk bete..."* instantly, then edits that message with the model's answer once it's ready
- **Long-answer handling:** responses over Discord's 2,000-character limit are split across follow-up messages
- **`!echo <phrase>`** repeats your phrase and deletes your original message
- Commands work with the `!` prefix **or** by @mentioning the bot
- Errors from the model or a command are reported back in the channel instead of failing silently

---

## 🏗 How it works

```
Discord user ──!Ola──▶ DiscordSocketClient ──▶ CommandService
                                                    │
                                                    ▼
                                              OlaCommand
                                                    │  streams tokens
                                                    ▼
                                    OllamaApiClient (localhost:11434)
                                                    │
                                                    ▼
                                          myllama3.1 (Llama 3.1)
```

The project uses **Microsoft.Extensions.DependencyInjection** to wire everything together:

| Service | Lifetime | Role |
|---|---|---|
| `DiscordSocketClient` | Singleton | Gateway connection (with the Message Content intent) |
| `CommandService` | Singleton | Discovers command modules by reflection and runs them |
| `OllamaApiClient` | Singleton | One shared connection to the local Ollama server, injected into commands |
| `IBot` → `Bot` | Singleton | Login, message routing, startup and shutdown |

Adding a new command means creating a class that extends `ModuleBase<SocketCommandContext>`. The `CommandService` picks it up automatically.

---

## 📁 Project structure

```
ChatBotDiscord/
├── Program.cs                     # DI container setup and app entry point
├── IBot.cs / Bot.cs               # Bot lifecycle and message → command routing
└── Commands/
    ├── Echo/EchoCommand.cs        # !echo
    └── Ollama/OlaCommand.cs       # !Ola — LLM replies via Ollama
```

---

## 🛠 Setup

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Ollama](https://ollama.com) installed and running
- A Discord bot application ([Developer Portal](https://discord.com/developers/applications))

### 1. Create the model in Ollama
The bot expects a model named **`myllama3.1`**. Create it from a Modelfile:

```bash
ollama pull llama3.1
ollama create myllama3.1 -f Modelfile
```

Example `Modelfile`:
```
FROM llama3.1
SYSTEM "You are a sarcastic Discord bot that roasts users while still answering their question."
```

### 2. Configure the Discord bot
In the Developer Portal, under **Bot**, turn on the **Message Content Intent**. Invite the bot with permission to read and send messages, plus **Manage Messages** if you want `!echo` to delete the original message.

### 3. Store your token with user-secrets
The token is never stored in the code:

```bash
git clone https://github.com/AdepuPranav/Rude_DiscordBot.git
cd Rude_DiscordBot/ChatBotDiscord
dotnet user-secrets set "DiscordToken" "YOUR_BOT_TOKEN"
```

### 4. Run
```bash
dotnet run
```
Press **Q** in the console to shut the bot down cleanly.

---

## 💬 Usage

```
!Ola what is a linked list
@RudeBot why is my code not working
!echo hello world
```

---

## 🔮 Possible improvements
- Per-channel conversation memory, so the bot remembers earlier messages
- Live-edit the reply as tokens stream in, instead of waiting for the full answer
- Slash commands (`/ola`) using Discord interactions
- Move the Ollama URL and model name into configuration

---

## ⚠ Disclaimer
The bot's personality is intentionally rude for entertainment. Use it only in servers where everyone is in on the joke.
