using System.Collections.Generic;
using UnityEngine;

public class PlayerFootsteps : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private CharacterController characterController;

    [Header("Clips")]
    [SerializeField] private AudioClip[] footstepClips;

    [Header("Timing")]
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float runStepInterval = 0.3f;
    [SerializeField] private float runSpeedThreshold = 4f;
    [SerializeField] private float minMoveSpeed = 0.1f;

    [Header("Randomization")]
    [SerializeField] private Vector2 volumeRange = new Vector2(0.8f, 1f);
    [SerializeField] private Vector2 pitchRange = new Vector2(0.9f, 1.1f);

    private float _stepTimer;
    private int _lastClipIndex = -1;

    private void Update()
    {
        float speed = new Vector3(characterController.velocity.x, 0f, characterController.velocity.z).magnitude;

        if (speed < minMoveSpeed)
        {
            _stepTimer = 0f;
            return;
        }

        float interval = speed >= runSpeedThreshold ? runStepInterval : walkStepInterval;
        _stepTimer += Time.deltaTime;

        if (_stepTimer >= interval)
        {
            _stepTimer = 0f;
            PlayFootstep();
        }
    }

    private void PlayFootstep()
    {
        if (footstepClips == null || footstepClips.Length == 0) return;

        int index = GetRandomIndex();
        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        audioSource.PlayOneShot(footstepClips[index], Random.Range(volumeRange.x, volumeRange.y));
        _lastClipIndex = index;
    }

    private int GetRandomIndex()
    {
        if (footstepClips.Length == 1) return 0;

        List<int> available = new List<int>();
        for (int i = 0; i < footstepClips.Length; i++)
        {
            if (i != _lastClipIndex)
                available.Add(i);
        }

        return available[Random.Range(0, available.Count)];
    }
}