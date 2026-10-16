namespace RiftHunters.Game
{
    // Optional intermission; PlanOleadas and GameManager remain the only wave sequencer.
    public interface IWaveRecovery
    {
        void BeginRecovery(bool final);
        void CancelRecovery(bool resetEco);
        bool ReadyToAdvance { get; }
    }
}
