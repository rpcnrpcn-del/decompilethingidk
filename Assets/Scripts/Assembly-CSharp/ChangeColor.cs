using UnityEngine;
using UnityEngine.EventSystems;

public class ChangeColor : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	private void OnEnable()
	{
	}

	public void SetRed(float value)
	{
		OnValueChanged(value, 0);
	}

	public void SetGreen(float value)
	{
		OnValueChanged(value, 1);
	}

	public void SetBlue(float value)
	{
		OnValueChanged(value, 2);
	}

	public void OnValueChanged(float value, int channel)
	{
		Color color = Color.white;
		if (GetComponent<Renderer>() != null)
		{
			color = GetComponent<Renderer>().material.color;
		}
		else if (GetComponent<Light>() != null)
		{
			color = GetComponent<Light>().color;
		}
		color[channel] = value;
		if (GetComponent<Renderer>() != null)
		{
			GetComponent<Renderer>().material.color = color;
		}
		else if (GetComponent<Light>() != null)
		{
			GetComponent<Light>().color = color;
		}
	}

	public void OnPointerClick(PointerEventData data)
	{
		if (GetComponent<Renderer>() != null)
		{
			GetComponent<Renderer>().material.color = new Color(Random.value, Random.value, Random.value, 1f);
		}
		else if (GetComponent<Light>() != null)
		{
			GetComponent<Light>().color = new Color(Random.value, Random.value, Random.value, 1f);
		}
	}
}
