﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Generation;

public class ShipCollision : MonoBehaviour {

	public TimeControl Control;
	public GameObject Model;
	public Movement Controls;
	public AudioSource CrashAudio;
	public World World;
	private bool _lock = false;

	void Start()
	{
		World = GameObject.FindGameObjectWithTag ("World").GetComponent<World>();
		Model = GameObject.FindGameObjectWithTag("Modelo");
	}

	public void Reset()
	{
		_lock = false;
	}

	void Update()
	{
		/*Vector3 BlockSpace = World.ToBlockSpace (transform.position);
		Chunk C = World.GetChunkAt (transform.position);
		if (C != null && C.GetBlockAt(BlockSpace) > 0) {
			DestroyShip ();
		}*/
	}


	void DestroyShip()
	{
		if (_lock)
			return;

		Controls.Lock ();
		Control.Lose ();
		Explode e = Model.AddComponent<Explode> ();
		e.ExplosionAudio = CrashAudio;
		_lock = true;
	}

	void OnCollisionEnter(Collision col)
	{
		DestroyShip ();
	}
}
