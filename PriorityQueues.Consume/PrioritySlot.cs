namespace PriorityQueues.Consume;
public class PrioritySlot(PriorityConsumer consumer, int quantum)
{
    public PriorityConsumer Consumer { get; } = consumer;

    /// <summary>
    /// The amount of accrued work this slot is allowed to do on the current round.
    /// </summary>
    public int Deficit { get; private set;  } = 0;

    /// <summary>
    /// Value which we increment <see cref="Deficit"/> by each round.
    /// </summary>
    public int Quantum { get; } = quantum;

    public void IncreaseDeficit()
    {
        Deficit += Quantum;
    }

    public void ReduceDeficit(int cost)
    {
        Deficit -= cost;
    }
}
