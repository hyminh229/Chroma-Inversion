using UnityEngine;

public class PlayerColorController : ChromaPolarityBase
{
    private void Update()
    {
        HandleColorSwitch();
    }

    private void HandleColorSwitch()
    {
        // Bỏ Space (giờ dành riêng cho Mega Beam) — chỉ còn chuột phải.
        if (Input.GetMouseButtonDown(1))
        {
            SwitchColor();
            Debug.Log("Player switched color to: " + CurrentColor);
        }
    }
}