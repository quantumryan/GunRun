using UnityEngine;
using System.Collections;

public class InputUtils
{
	/// <summary>
	/// Checks that only the modifier keys you want are used.
	/// </summary>
	/// <param name="control"></param>
	/// <param name="alt"></param>
	/// <param name="shift"></param>
	/// <returns></returns>
	public static bool CheckKeyModifierCombo(bool control, bool alt, bool shift)
	{
		return (!alt ^ (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))) &&
			(!control ^ (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) &&
			(!shift ^ (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)));
	}

	/// <summary>
	/// Only returns true if a control key is down. Not true if other modifier keys (alt or shift) are also down.
	/// </summary>
	/// <returns></returns>
	public static bool GetKeyControl()
	{
		return CheckKeyModifierCombo(true, false, false);
	}
	public static bool GetKeyDownControl(KeyCode keycodedown)
	{
		return Input.GetKeyDown(keycodedown) && GetKeyControl();
	}

	/// <summary>
	/// Only returns true if a alt key is down. Not true if other modifier keys (control or shift) are also down.
	/// </summary>
	/// <returns></returns>
	public static bool GetKeyAlt()
	{
		return CheckKeyModifierCombo(false, true, false);
	}
	public static bool GetKeyDownAlt(KeyCode keycodedown)
	{
		return Input.GetKeyDown(keycodedown) && GetKeyAlt();
	}

	/// <summary>
	/// Only returns true if a shift key is down. Not true if other modifier keys (control or alt) are also down.
	/// </summary>
	/// <returns></returns>
	public static bool GetKeyShift()
	{
		return CheckKeyModifierCombo(false, false, true);
	}
	public static bool GetKeyDownShift(KeyCode keycodedown)
	{
		return Input.GetKeyDown(keycodedown) && GetKeyShift();
	}


	/// <summary>
	/// Only returns true if control-alt keys are down. Not true if a shift key is also down.
	/// </summary>
	/// <returns></returns>
	public static bool GetKeyControlAlt()
	{
		return CheckKeyModifierCombo(true, true, false);
	}
	public static bool GetKeyDownControlAlt(KeyCode keycodedown)
	{
		return  Input.GetKeyDown(keycodedown) && GetKeyControlAlt();
	}
	
	
	/// <summary>
	/// Provides a simple check of the Time.timeScale to determine if raw inputs should be used instead
	/// of the provided smoothing. Intended for switching to GetAxisRaw() when timeScale is 0, because the 
	/// smoothing of GetAxis() won't apply because of 0 time.
	/// </summary>
	/// <returns>The float value of the axis named, Raw or not depending on current timeScale.</returns>
	/// <param name="axisName">Axis name.</param>
	/// <param name="rawTimeUnder">Optional time value. Any time scale under this value will return the raw axis.
	/// (Default = 1.0 second)</param>
	public static float GetAxisRate(string axisName, float rawTimeUnder = 1.0f)
	{
		if(Time.timeScale < rawTimeUnder)
		{
			return Input.GetAxisRaw(axisName);
		}
		
		return Input.GetAxis(axisName);
	}


	/// <summary>
	/// For text input, making it a float.
	/// </summary>
	/// <returns></returns>
	public static float GetTextAsFloat(string text, float failReturn = 1.0f)
	{
		float val = 0.0f;
		if (!float.TryParse(text, out val))
		{
			//Debug.Log("Not valid parse. " + text + "  " + lastCommittedText);
			text = failReturn.ToString("0.000");
			val = failReturn;
		}
		else
		{
			//Debug.Log("Valid parse. " + text + "  " + lastCommittedText);
			text = val.ToString("0.000");
		}
		return val;
	}
}
