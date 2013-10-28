using UnityEngine;
using System;
using System.Collections.Generic;

public class Slot : FContainer
{
	public Player player;

	public HandleBox handleBox;
	public NameBox nameBox;
	public ScoreBox scoreBox;
	public MathBox minusBox;
	public MathBox plusBox;

	private bool _isMathMode;

	private float _width;
	private float _height;

	private bool _hasHandle = false;

	public RXTweenable buildIn;

	public int index = -1;

	public Slot(Player player, float width, float height)
	{
		this.player = player;

		_width = width;
		_height = height;

		AddChild(handleBox = new HandleBox(this));
		AddChild(nameBox = new NameBox(this));
		AddChild(scoreBox = new ScoreBox(this));
		AddChild(minusBox = new MathBox(this, MathType.Minus));
		AddChild(plusBox = new MathBox(this, MathType.Plus));

//		Box box = new Box();
//		AddChild(box);
//		box.Init(Player.NullPlayer);
//		box.SetSize(_width,_height);
//		box.y -= _height;

		minusBox.SignalTick += HandleMinusTick;
		plusBox.SignalTick += HandlePlusTick;
		nameBox.SignalRelease += HandleNameTap;

		ListenForUpdate(HandleUpdate);
		DoLayout();

		buildIn = new RXTweenable(0.0f, HandleBuildInChange);
		HandleBuildInChange();
	}

	public void DoLayout()
	{
		float padding = Config.PADDING_S;
		Vector2 cursor = new Vector2(-_width*0.5f, _height*0.5f); //the top left

		float freeWidth = _width;

		freeWidth -= minusBox.GetNeededWidth()+padding; //minus width
		freeWidth -= plusBox.GetNeededWidth()+padding; //plus width
		freeWidth -= padding; //padding between name and score

		if(_hasHandle)
		{
			AddChild(handleBox);
			handleBox.SetSize(handleBox.GetNeededWidth(),_height);
			handleBox.SetTopLeft(cursor.x,cursor.y);
			cursor.x += handleBox.width + padding;
			freeWidth -= handleBox.width+padding; //handle width
		}
		else 
		{
			handleBox.RemoveFromContainer();
		}

		float mathModeMultiplier = (1.0f + 0.5f*scoreBox.mathModeTweenAmount); //between 1.0f and 2.0f
		float maxScoreWidth = 100.0f;
		float scoreWidth = Mathf.Min(maxScoreWidth,freeWidth * 2.0f/5.0f) * mathModeMultiplier;
		float nameWidth = freeWidth - scoreWidth;

		nameBox.SetSize(nameWidth,_height);
		nameBox.SetTopLeft(cursor.x,cursor.y);
		cursor.x += nameBox.width + padding;

		scoreBox.SetSize(scoreWidth,_height);
		scoreBox.SetTopLeft(cursor.x,cursor.y);
		cursor.x += scoreBox.width + padding;

		minusBox.SetSize(minusBox.GetNeededWidth(),_height);
		minusBox.SetTopLeft(cursor.x,cursor.y);
		cursor.x += minusBox.width + padding;

		plusBox.SetSize(plusBox.GetNeededWidth(),_height);
		plusBox.SetTopLeft(cursor.x,cursor.y);
		cursor.x += plusBox.width + padding;
		
		//score
		//name
		//plus
		//minus
	}

	private void HandleBuildInChange()
	{
		float amount = buildIn.amount;

		handleBox.scale = RXEase.ExpoOut(RXMath.GetSubPercent(amount, 0.0f,0.4f));
		nameBox.scale = RXEase.ExpoOut(RXMath.GetSubPercent(amount, 0.15f,0.55f));
		scoreBox.scale = RXEase.ExpoOut(RXMath.GetSubPercent(amount, 0.3f,0.7f));
		minusBox.scale = RXEase.ExpoOut(RXMath.GetSubPercent(amount, 0.45f,0.85f));
		plusBox.scale = RXEase.ExpoOut(RXMath.GetSubPercent(amount, 0.6f,1.0f));

//		Debug.Log("m " + nameBox.scale);
	}

	private void HandleNameTap()
	{
		FSoundManager.PlaySound("UI/Button1");
		nameBox.DoTapEffect();

		Keeper.instance.slotList.RemoveSlotForPlayer(this.player,false,true);

//		player.name = RXRandom.GetRandomItem("BELLA", "JOHNNY", "darko", "wallice fourteen", "everyone", "johnny b", "wick","j","","             ", "do ya", "hollaber four") as string;
	}

	private void HandleMinusTick(int ticks) 
	{
		StartMathMode();
		player.score -= ticks;
	}

	private void HandlePlusTick(int ticks)
	{
		StartMathMode();

		player.score += ticks;
	}


	private void HandleUpdate()
	{
		if(_mathModeAmount > 0)
		{
			_mathModeAmount -= Time.deltaTime/Config.MATH_MODE_TIME;
			if(_mathModeAmount <= 0)
			{
				CloseMathMode();
			}
		}

		if(_isMathMode || scoreBox.mathModeTweenAmount > 0)
		{
			DoLayout();
		}
	}

	private float _mathModeAmount; 

	public void StartMathMode()
	{
		_mathModeAmount = 1.0f;

		if(!_isMathMode) //just increment the timer
		{
			_isMathMode = true;
			Go.killAllTweensWithTarget(scoreBox);
			Go.to(scoreBox, 0.25f, new TweenConfig().floatProp("mathModeTweenAmount",1.0f).setEaseType(EaseType.ExpoOut).onComplete(HandleMathModeOpenComplete));
			FSoundManager.PlaySound("UI/MathOpen");
		}
	}

	void HandleMathModeOpenComplete(AbstractTween obj)
	{

	}

	private void CloseMathMode()
	{
		_isMathMode = false;
		Go.killAllTweensWithTarget(scoreBox);
		Go.to(scoreBox, 0.25f, new TweenConfig().floatProp("mathModeTweenAmount",0.0f).setEaseType(EaseType.ExpoOut).onComplete(HandleMathModeCloseComplete));
		FSoundManager.PlaySound("UI/MathClose");
	}
	
	void HandleMathModeCloseComplete(AbstractTween obj)
	{
		_isMathMode = false;
		Keeper.instance.slotList.Reorder(true,false,true);
	}


	public void PauseMathMode()
	{
		throw new NotImplementedException();
	}

	public void ResumeMathMode()
	{
		throw new NotImplementedException();
	}

	public float width
	{
		get {return _width;}
		set {if(_width != value) {_width = value; DoLayout();}}
	}
	
	public float height
	{
		get {return _height;}
		set {if(_height != value) {_height = value; DoLayout();}}
	}

	public bool isMathMode
	{
		get {return _isMathMode;}
	}
}




