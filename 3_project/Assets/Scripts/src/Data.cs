using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Debug = UnityEngine.Debug;

public enum Listtype { Elapsed, Pressure, Flow, BasketTemperature, MixTemperature }

/// <summary>
/// Ein Data Objekt beinhalten genau einen Datensatz. Der Path wird bei Initialisierung dem Konstruktor übergeben.
/// </summary>
public class Data
{
	/*
	 * Structure:
	 *	Date
	 *	Timestamp
	 *	Elapsed
	 *	timers
	 *	pressure (pressure, goal)
	 *	flow (flow, by_weight, by_weight_raw, goal)
	 *	temperature (basket, mix, goal)
	 *	scale
	 *	totals (weight, water_dispensed)
	 *	resistance (resistance, by_weight)
	 *	state_change
	 *	...
	 *	
	 * 
	 */
	private string Date { get; set; }

	private String _name;
	private long Timestamp { get; set; }

	private List<float> elapsed;

	private List<float> pressure;

	private List<float> flow;

	private List<float> basketTemperature;

	private List<float> mixTemperature;


	public Data(string path)
	{
		elapsed = new List<float>();
		pressure = new List<float>();
		flow = new List<float>();
		basketTemperature = new List<float>();
		mixTemperature = new List<float>();
		_name = path.Substring(path.LastIndexOf("/"));
		
        string text = File.ReadAllText(path);
		JsonParser(text);

		// Setting the starting point to 0
		elapsed[0] = 0;
    }
	public string GetName()
    {
	    return _name;
    }

	public List<float> getList(Listtype listtype)
	{
		switch (listtype)
		{
			case Listtype.Elapsed:
				return elapsed;
			case Listtype.Pressure:
				return pressure;
			case Listtype.Flow:
				return flow;
			case Listtype.BasketTemperature:
				return basketTemperature;
			default:
				return mixTemperature;
		}
	}

	/// <summary>
	/// Übersetzt einen Json String in Daten. 
	/// </summary>
	/// <param name="json"></param>
	private void JsonParser(string json)
	{
		this.Date = json.Substring(json.IndexOf("date") + "date".Length + 4, json.IndexOf("timestamp") - ("date".Length + 4) - json.IndexOf("date") - 6);

		this.Timestamp = long.Parse(json.Substring(json.IndexOf("timestamp") + "timestamp".Length + 4, json.IndexOf("elapsed") - ("timestamp".Length + 4) - json.IndexOf("timestamp") - 4));

		string[] eps = json.Substring(json.IndexOf("elapsed") + "elapsed".Length + 4, json.IndexOf("timers") - ("elapsed".Length + 4) - json.IndexOf("elapsed") - 4).Split(',');
		foreach (string el in eps)
            this.elapsed.Add(float.Parse(el.Trim(new char[] { '"',' '}), CultureInfo.InvariantCulture.NumberFormat));

        string[] psr = json.Substring(json.IndexOf("pressure") + 2 * ("pressure".Length + 4) + 1, json.IndexOf("goal") - 2 * ("pressure".Length + 4) - 1 - json.IndexOf("pressure") - 4).Split(',');
        foreach (string el in psr)
            this.pressure.Add(float.Parse(el.Trim(new char[] { '"', ' ' }), CultureInfo.InvariantCulture.NumberFormat));

        string[] flw = json.Substring(json.IndexOf("flow") + 2 * ("flow".Length + 4) + 1, json.IndexOf("by_weight") - 2 * ("flow".Length + 4) - 1 - json.IndexOf("flow") - 4).Split(',');
        foreach (string el in flw)
            this.flow.Add(float.Parse(el.Trim(new char[] { '"', ' ' }), CultureInfo.InvariantCulture.NumberFormat));

        string[] bTemp = json.Substring(json.IndexOf("temperature") + ("temperature".Length + 4) + ("basket".Length + 4) + 1, json.IndexOf("mix") - ("basket".Length + 4 + "temperature".Length + 4) - 1 - json.IndexOf("temperature") - 4).Split(',');
		foreach (string el in bTemp)
            this.basketTemperature.Add(float.Parse(el.Trim(new char[] { '"', ' ' }), CultureInfo.InvariantCulture.NumberFormat));

		json = json.Remove(0, json.IndexOf("temperature"));
        string[] mTemp = json.Substring(json.IndexOf("mix") + ("mix".Length + 4) + 1, json.IndexOf("goal") - ("mix".Length + 4) - 1 - json.IndexOf("mix") - 4).Split(',');
        foreach (string el in mTemp)
            this.mixTemperature.Add(float.Parse(el.Trim(new char[] { '"', ' ' }), CultureInfo.InvariantCulture.NumberFormat));
    }

	//Problem : Falls time < erster Wert!!
	/// <summary>
	/// Bekommt einen Wert und gibt kleinstes Intervall von elapsed zurück
	/// </summary>
	/// <param name="time"></param>
	/// <returns>kleinst Mögliche Intervallgrenze</returns>
	public float[] getTimeInterval(float time)
	{
		//Idee: Falls Zeitraum abgelaufen ist, einfach von vorne anfangen (Modulo verwenden)
		float lowerBound = 0;
		float upperBound = -1;

		foreach (float el in this.elapsed)
		{
			if (el < time)
				lowerBound = el;
			if (el > time)
			{
				upperBound = el;
				break;
            }
		}
		
		float[] val = new float[2];
		val[0] = lowerBound;
		val[1] = upperBound;

        return val;
	}
	/*
	 * This Should Not be implemented anywhere anymore!!!
	 * Stattdessen bitte die Manager.getValues Methode benutzen

	/// <summary>
	/// GetValue nimmt einen Timestamp und gibt alle Daten zu diesem Timestamp zurück.
	/// Überprüfe ob -1 zurückgegeben wurde (Datenreihe zu Ende)
	/// </summary>
	/// <param name="time"> ein Zeitpunkt, falls zu groß wird -1 gesetzt</param>
	/// <returns>Liste von Floats: Pressure, Flow, BasketTemperature, Mix Temperature</returns>
	public List<float> getValue(float time)
	{
		float[] timeInterval = getTimeInterval(time);
		if (timeInterval[1] == -1)
			return new List<float>(){ -1,-1,-1,-1 };

		List<float> values = new List<float>();

		// Identify time interval
		int[] time_index = new int[2];
		time_index[0] = elapsed.IndexOf(timeInterval[0]);
        time_index[1] = elapsed.IndexOf(timeInterval[1]);

        float normTime = (time - timeInterval[0]) / (timeInterval[1] - timeInterval[0]);

		//Pressure
		float lowerPressure = this.pressure[time_index[0]];
		float upperPressure = this.pressure[time_index[1]];

		float pressureValue = normTime * (upperPressure - lowerPressure) + lowerPressure;

        //Flow
        float lowerFlow = this.flow[time_index[0]];
        float upperFlow = this.flow[time_index[1]];

        float flowValue = normTime * (upperFlow - lowerFlow) + lowerFlow;

        //Basket Temperature
        float lowerBask = this.basketTemperature[time_index[0]];
        float upperBask = this.basketTemperature[time_index[1]];

        float baskValue = normTime * (upperBask - lowerBask) + lowerBask;

        //Mix Temperature
        float lowerMix = this.mixTemperature[time_index[0]];
        float upperMix = this.mixTemperature[time_index[1]];

        float mixValue = normTime * (upperMix - lowerMix) + lowerMix;

		// Add to list
		values.Add(pressureValue);
        values.Add(flowValue);
        values.Add(baskValue);
        values.Add(mixValue);
        
		return values;
    }
	*/
	
	/// <summary>
	/// Bekommt einen Parameter in Form eines <paramref name="listtype"/> und gibt ein Array mit 2 einträgen (min, max) zurück
	/// </summary>
	/// <param name="listtype">Name des Parameters als Enum</param>
	/// <returns>ein Array [min,max] des jeweiligen Parameters</returns>
	public float[] getMinMax(Listtype listtype)
	{
		switch (listtype)
		{
			case Listtype.Elapsed:
				{
					float[] val = new float[2];
					val[0] = elapsed.Min();
					val[1] = elapsed.Max();
					return val;
                }
            case Listtype.Pressure:
                {
                    float[] val = new float[2];
                    val[0] = pressure.Min();
                    val[1] = pressure.Max();
                    return val;
                }
            case Listtype.Flow:
                {
                    float[] val = new float[2];
                    val[0] = flow.Min();
                    val[1] = flow.Max();
                    return val;
                }
            case Listtype.BasketTemperature:
                {
                    float[] val = new float[2];
                    val[0] = basketTemperature.Min();
                    val[1] = basketTemperature.Max();
                    return val;
                }
			default:
                {
                    float[] val = new float[2];
                    val[0] = mixTemperature.Min();
                    val[1] = mixTemperature.Max();
                    return val;
                }
        }
			
	}
}
