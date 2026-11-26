namespace DominoTrainGame.Shared.Rules;

public static class MatchRules
{
    public const int DoubleSixPips = 6;
    public const int DoubleNinePips = 9;
    public const int DoubleTwelvePips = 12;
    public const int MexicanTrainOwnerId = 0;

    public static bool IsMexicanTrain(int trainOwnerId)
    {
        bool isMexicanTrain = trainOwnerId == MexicanTrainOwnerId;
        return isMexicanTrain;
    }
}
