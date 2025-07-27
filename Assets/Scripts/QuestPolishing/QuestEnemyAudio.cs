using System;
using UnityEngine;

[DisallowMultipleComponent]
public class QuestEnemyAudio : MonoBehaviour
{
    [Serializable]
    public class QuestEnemyAudioClip : RecRoomAudioClip
    {
        // Token: 0x04000289 RID: 649
        public bool isLooping;
    }

    [SerializeField]
    private RecRoomAudioClip spawnVO;

    [SerializeField]
    private RecRoomAudioClip deathVO;

    [SerializeField]
    private QuestEnemyAudioClip movingSound;

    [SerializeField]
    private RecRoomAudioClip[] attackVOs;

    [SerializeField]
    private QuestEnemyAudioClip[] attackSounds;

    [SerializeField]
    private RecRoomAudioClip[] takeDamageVOs;

    private RigidbodySteering enemySteering;

    private SFXAudioSource movingSoundAudioSource;

    private SFXAudioSource attackingSoundAudioSource;

    private void Awake()
    {
        this.enemySteering = base.GetComponent<RigidbodySteering>();
    }
    private void Update()
    {
        if (this.movingSoundAudioSource != null && this.movingSoundAudioSource.AudioSource.isPlaying && this.enemySteering != null)
        {
            this.movingSoundAudioSource.AudioSource.volume = Mathf.Min(1f, this.enemySteering.CurrentVelocity.magnitude / this.enemySteering.MaxLinearSpeed);
        }
    }
    public void OnSpawn()
    {
        AudioManager.Play3DSFX(this.spawnVO, base.transform, null);
    }
    public void OnDeath()
    {
        AudioManager.Play3DSFX(this.deathVO, base.transform, null);
        if (this.movingSoundAudioSource != null)
        {
            this.movingSoundAudioSource.Release();
            this.movingSoundAudioSource = null;
        }
        if (this.attackingSoundAudioSource != null)
        {
            this.attackingSoundAudioSource.Release();
            this.attackingSoundAudioSource = null;
        }
    }
    public void OnMove()
    {
        if (this.movingSound.isLooping && this.movingSoundAudioSource == null)
        {
            this.movingSoundAudioSource = AudioManager.StartLooping3DSFX(this.movingSound, base.transform, null);
        }
        else
        {
            AudioManager.Play3DSFX(this.movingSound, base.transform, null);
        }
    }
    public void OnAttack(int soundIndex = -1)
    {
        AudioManager.PlayRandom3DSFX(this.attackVOs, base.transform, null, false);
        if (this.attackSounds != null && this.attackSounds.Length > 0)
        {
            int num = ((soundIndex < 0) ? global::UnityEngine.Random.Range(0, this.attackSounds.Length - 1) : soundIndex);
            QuestEnemyAudioClip questEnemyAudioClip = this.attackSounds[num];
            if (questEnemyAudioClip.isLooping && this.attackingSoundAudioSource == null)
            {
                this.attackingSoundAudioSource = AudioManager.StartLooping3DSFX(questEnemyAudioClip, base.transform, null);
            }
            else
            {
                AudioManager.Play3DSFX(questEnemyAudioClip, base.transform, null);
            }
        }
    }
    public void OnAttackStop()
    {
        if (this.attackingSoundAudioSource != null && this.attackingSoundAudioSource.AudioSource.isPlaying)
        {
            this.attackingSoundAudioSource.Release();
        }
    }
    public void OnTakeDamage()
    {
        AudioManager.PlayRandom3DSFX(this.takeDamageVOs, base.transform, null, false);
    }
}
