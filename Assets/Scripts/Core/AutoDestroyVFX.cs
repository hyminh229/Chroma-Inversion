using UnityEngine;

public class AutoDestroyVFX : MonoBehaviour
{
    [Tooltip("Thời gian tự hủy nếu không có Animation (giây)")]
    [SerializeField] private float lifetime = 0.5f;

    [Tooltip("Tự động tính thời gian theo độ dài của Animation clip (nếu có Animator)")]
    [SerializeField] private bool useAnimationDuration = true;

    private void Start()
    {
        float delay = lifetime;

        if (useAnimationDuration && TryGetComponent(out Animator animator))
        {
            AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
            if (clipInfo.Length > 0 && clipInfo[0].clip != null)
            {
                delay = clipInfo[0].clip.length;
            }
        }

        Destroy(gameObject, delay);
    }
}
