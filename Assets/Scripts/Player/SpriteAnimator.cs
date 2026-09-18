// ============================================================================
// SpriteAnimator.cs
// 스프라이트 애니메이션 시스템 (SpriteRenderer 전용)
// Unity Animator 없이 스프라이트 시퀀스 재생
// ============================================================================

using UnityEngine;
using System;
using System.Collections.Generic;

public class SpriteAnimator : MonoBehaviour
{
    // ========================================================================
    // 애니메이션 데이터
    // ========================================================================
    [System.Serializable]
    public class AnimationSequence
    {
        public string name;
        public Sprite[] frames;
        public float frameTime = 0.05f;  // 20fps로 설정 (잔상 방지)
        public bool loop = true;
    }
    
    // ========================================================================
    // 설정
    // ========================================================================
    [Header("Animation Sequences")]
    [Tooltip("Inspector에서 직접 설정하거나 AnimationLoader가 자동 할당합니다.")]
    public AnimationSequence[] sequences = Array.Empty<AnimationSequence>();
    
    [Header("Settings")]
    public string defaultAnimation = "ready";
    
    // ========================================================================
    // 런타임 상태
    // ========================================================================
    private SpriteRenderer spriteRenderer;
    private AnimationSequence currentSequence;
    private int currentFrame;
    private float timer;
    private bool isPlaying;
    
    // ========================================================================
    // 외부 참조
    // ========================================================================
    public SpriteRenderer SpriteRenderer => spriteRenderer;
    public string CurrentAnimation => currentSequence?.name;
    public bool IsPlaying => isPlaying;
    
    // ========================================================================
    // 초기화
    // ========================================================================
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    private void Start()
    {
        if (sequences != null && sequences.Length > 0)
        {
            Play(defaultAnimation);
        }
    }
    
    // ========================================================================
    // 애니메이션 시퀀스 설정 (AnimationLoader에서 호출)
    // ========================================================================
    public void SetSequences(AnimationSequence[] newSequences)
    {
        sequences = newSequences;
        if (sequences != null && sequences.Length > 0)
        {
            Play(defaultAnimation);
        }
    }
    
    public void SetSequences(List<AnimationSequence> newSequences)
    {
        sequences = newSequences.ToArray();
        if (sequences != null && sequences.Length > 0)
        {
            Play(defaultAnimation);
        }
    }
    
    // ========================================================================
    // 업데이트
    // ========================================================================
    private void Update()
    {
        if (!isPlaying || currentSequence == null) return;
        if (currentSequence.frames == null || currentSequence.frames.Length == 0) return;
        
        timer += Time.deltaTime;
        
        if (timer >= currentSequence.frameTime)
        {
            timer = 0f;
            currentFrame++;
            
            if (currentFrame >= currentSequence.frames.Length)
            {
                if (currentSequence.loop)
                {
                    currentFrame = 0;
                }
                else
                {
                    currentFrame = currentSequence.frames.Length - 1;
                    isPlaying = false;
                    return;
                }
            }
            
            Sprite sprite = currentSequence.frames[currentFrame];
            if (sprite != null)
            {
                SetSprite(sprite);
            }
        }
    }
    
    // ========================================================================
    // 스프라이트 설정
    // ========================================================================
    private void SetSprite(Sprite sprite)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }
    
    // ========================================================================
    // 애니메이션 재생
    // ========================================================================
    public void Play(string animationName)
    {
        AnimationSequence seq = FindSequence(animationName);
        if (seq == null)
        {
            Debug.LogWarning($"[SpriteAnimator] 애니메이션을 찾을 수 없음: {animationName}");
            return;
        }
        
        if (currentSequence != seq)
        {
            currentSequence = seq;
            currentFrame = 0;
            timer = 0f;
            isPlaying = true;
            
            if (seq.frames.Length > 0 && seq.frames[0] != null)
            {
                SetSprite(seq.frames[0]);
            }
        }
        else if (!isPlaying)
        {
            isPlaying = true;
        }
    }
    
    public void Play(string animationName, bool forceLoop)
    {
        AnimationSequence seq = FindSequence(animationName);
        if (seq != null)
        {
            bool originalLoop = seq.loop;
            seq.loop = forceLoop;
            Play(animationName);
            seq.loop = originalLoop;
        }
    }
    
    public void Stop()
    {
        isPlaying = false;
    }
    
    public void Pause()
    {
        isPlaying = false;
    }
    
    public void Resume()
    {
        if (currentSequence != null)
            isPlaying = true;
    }
    
    // ========================================================================
    // 유틸리티
    // ========================================================================
    private AnimationSequence FindSequence(string name)
    {
        if (sequences == null) return null;
        
        foreach (var seq in sequences)
        {
            if (seq.name == name)
                return seq;
        }
        return null;
    }
    
    public int GetFrameCount(string animationName)
    {
        AnimationSequence seq = FindSequence(animationName);
        return seq?.frames?.Length ?? 0;
    }
    
    public bool IsAnimationEnded(string animationName)
    {
        if (currentSequence?.name != animationName) return true;
        return !isPlaying && currentFrame >= currentSequence.frames.Length - 1;
    }
    
    public void SetFlipX(bool flip)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = flip;
        }
    }
    
    public bool GetFlipX()
    {
        if (spriteRenderer != null)
        {
            return spriteRenderer.flipX;
        }
        return false;
    }
    
    public AnimationSequence[] GetSequences()
    {
        return sequences;
    }
}
