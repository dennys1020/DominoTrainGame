using System;
using DominoTrainGame.Server.Application.Ports;

namespace DominoTrainGame.Server.Application.UseCases;

public sealed class LobbyInputRules
{
    public LobbyInputRules(
        ILobbyCodeGenerator codeGenerator,
        IChatMessageFilter messageFilter,
        IChatSanctionUseCases chatSanctions)
    {
        CodeGenerator = codeGenerator ?? throw new ArgumentNullException(nameof(codeGenerator));
        MessageFilter = messageFilter ?? throw new ArgumentNullException(nameof(messageFilter));
        ChatSanctions = chatSanctions ?? throw new ArgumentNullException(nameof(chatSanctions));
    }

    public ILobbyCodeGenerator CodeGenerator { get; }

    public IChatMessageFilter MessageFilter { get; }

    public IChatSanctionUseCases ChatSanctions { get; }
}
