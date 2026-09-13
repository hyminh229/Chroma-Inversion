using System;

// Interface chung cho mọi loại wave — WaveSequencer chỉ cần biết 2 thứ này,
// không quan tâm bên trong từng wave spawn kiểu gì.
public interface IWaveSpawner
{
    event Action OnWaveCleared;
    void StartWave();
}