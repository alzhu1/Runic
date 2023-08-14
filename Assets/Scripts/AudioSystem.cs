using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound {
    public string name;

    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume;

    public bool loop;
    
    public float pitch;

    [HideInInspector]
    public AudioSource source;
}

public class AudioSystem : MonoBehaviour {
    // Expose as singleton, but not public (use events to trigger audio)
    private static AudioSystem instance = null;

    [SerializeField] private Sound[] sounds;

    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
            return;
        }

        foreach (Sound s in sounds) {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.loop = s.loop;
            s.source.pitch = s.pitch;

            s.source.playOnAwake = false;
            s.source.bypassEffects = true;
            s.source.bypassListenerEffects = true;
            s.source.bypassReverbZones = true;
        }

        Play("BGM");
    }

    void Start() {
        // Attach events
        EventBus.instance.OnPlayerAction += ReceivePlayerActionEvent;
        EventBus.instance.OnRuneCollected += ReceiveRuneCollectedEvent;
    }

    void OnDestroy() {
        // Detach events
        EventBus.instance.OnPlayerAction -= ReceivePlayerActionEvent;
        EventBus.instance.OnRuneCollected -= ReceiveRuneCollectedEvent;
    }

    Sound Play(string name) {
        Sound s = System.Array.Find<Sound>(sounds, sound => sound.name.Equals(name));
        if (s == null || s.source == null) {
            Debug.LogWarning($"Sound {name} does not exist!");
        } else {
            s?.source.Play();
        }
        return s;
    }

    void ReceivePlayerActionEvent(int abilityId, bool state) {
        switch (abilityId) {
            case Player.JUMP_ID: {
                Play("Jump");
                break;
            }

            case Player.DASH_ID: {
                Play("Dash");
                break;
            }

            case Player.CHANGE_SIZE_ID: {
                Play(state ? "Shrink" : "Grow");
                break;
            }

            case Player.SUPER_JUMP_ID: {
                Play("SuperJump");
                break;
            }
        }
    }

    void ReceiveRuneCollectedEvent(Rune r) {
        Play("RuneCollected");
    }
}
