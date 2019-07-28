using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GameOptions
{
	public int nodeCount;
	public int nodeMemoryLength;
	public int readMemoryCount;
	public int dataLength;
	public int memoryCellCount;
	public int setAddressChars;
	public int labelLength;
	public int maxLog;
	public float codeFrameDelay;
    public float timeOut;
	public float packetTravelTime;

	public double alphaIntMaxValue;

	public GameOptions(	int nC,
						int nML,
						int rMC,
						int dL,
						int mCC,
						int sAC,
						int lL,
						int mL,
						float cFD,
                        float tO,
						float pTT)
	{
		nodeCount = nC;
		nodeMemoryLength = nML;
		readMemoryCount = rMC;
		dataLength = dL;
		memoryCellCount = mCC;
		setAddressChars = sAC;
		labelLength = lL;
		maxLog = mL;
		codeFrameDelay = cFD;
        timeOut = tO;
		packetTravelTime = pTT;

		alphaIntMaxValue = Math.Pow(26, dataLength)-1;
	}
}

public class GameInitScript : MonoBehaviour
{
	public int nodeCount = 1000;
	public int nodeMemoryLength = 100;
	public int readMemoryCount = 10;
	public int dataLength = 8; // Change with caution
	public int memoryCellCount = 4;
	public int numberOfSetAddressChars = 4;
	public int labelLength = 6;
	public int maxLog = 100;
	public float codeFrameDelay = 0.1f;
    public float timeOut = 6;
	public float packetTravelTime = 4;

	// Use this for initialization
	void Start ()
	{
		GameOptions gameOptions = new GameOptions(
			nodeCount,
			nodeMemoryLength,
			readMemoryCount,
			dataLength,
			memoryCellCount,
			numberOfSetAddressChars,
			labelLength,
			maxLog,
			codeFrameDelay,
            timeOut,
			packetTravelTime
		);

		GameManager.Init(gameOptions);
		NodeManager.Init();
		PacketManager.Init();
		NodeGraphicManager.Init();
		PacketGraphicManager.Init();

		GameManager.InitPlayers();
	}
	
	void Update()
	{
		GameManager.CodeUpdate();
	}
}
