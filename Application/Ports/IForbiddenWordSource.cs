using System.Collections.Generic;

namespace DominoTrainGame.Server.Application.Ports;

public interface IForbiddenWordSource
{
    IReadOnlyList<string> LoadEntries();
}
