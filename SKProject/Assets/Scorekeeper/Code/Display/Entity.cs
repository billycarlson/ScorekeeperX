using UnityEngine;
using System;
using System.Collections.Generic;

public class Entity : FContainer
{
	public List<Box> boxes = new List<Box>();
	public List<FSprite> contentSprites = new List<FSprite>();
	public FContainer contentContainer;

	protected Player _player;

	protected Cell _baseCell = new Cell();
	protected Cell _currentCell = new Cell();
	protected Cell _targetCell = new Cell();

	protected float _tweenTimeElapsed;
	protected float _tweenTimeTotal;

	protected float _percent = 0.0f;

	public Entity()
	{

	}
	
	virtual public void Init(Player player)
	{
		Box box = new Box();
		AddChild(box);
		boxes.Add(box);

		AddChild(contentContainer = new FContainer());

		_player = player;
		UpdatePlayer();

		ListenForAfterUpdate(HandleAfterUpdate);
	}

	void HandleAfterUpdate ()
	{
		if(_targetCell.didHaveMajorChange)
		{
			_baseCell = _currentCell.Clone();
			_tweenTimeElapsed = Mathf.Max(0.0f, _tweenTimeElapsed-0.3f);
			_tweenTimeTotal = Mathf.Max(0.3f, _tweenTimeTotal); //if there has been a major change, at least tween a bit
		}

		if(_tweenTimeElapsed < _tweenTimeTotal)
		{
			_tweenTimeElapsed += Time.deltaTime;
		}

		UpdatePosition();
	}

	virtual public void UpdatePosition()
	{
		_percent = Mathf.Clamp01(_tweenTimeElapsed / _tweenTimeTotal);

		if(_percent > 0 && _percent < 1.0f)
		{
			_percent = RXEase.ExpoInOut(_percent);
		}

		_currentCell.SetInterpolated(_baseCell,_targetCell,_percent);

		this.x = _currentCell.x;
		this.y = _currentCell.y;
		boxes[0].width = _currentCell.width;
		boxes[0].height = _currentCell.height;
		this.rotation = _currentCell.rotation;
		this.alpha = _currentCell.alpha;
	}

	protected void UpdatePlayer ()
	{
		boxes.ForEach(box => {box.color = _player.color.color;});
	}

	public void MoveToCellTweened(Cell cell, float tweenTime)
	{
		_tweenTimeTotal = tweenTime;
		_tweenTimeElapsed = 0.0f;
		_baseCell = _currentCell.Clone();
		_targetCell = cell;
	}

	public void MoveToCellInstantly(Cell cell)
	{
		_tweenTimeTotal = 0.0f;
		_tweenTimeElapsed = 0.001f;
		_baseCell = _currentCell.Clone();
		_targetCell = cell;
		UpdatePosition();
	}

	public Player player 
	{
		get {return _player;}
		set 
		{
			if(_player != value)
			{
				_player = value;
				UpdatePlayer();
			}
		}
	}

	public Cell currentCell
	{
		get {return _currentCell;}
	}

	public Cell baseCell
	{
		get {return _baseCell;}
	}

	public Cell targetCell
	{
		get {return _targetCell;}
	}
}
