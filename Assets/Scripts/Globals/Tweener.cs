using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*public abstract class BaseArithmeticable<T>
{
	public abstract T add(BaseArithmeticable<T> right);
	public abstract T add(int right);
	public abstract T add(float right);
	public abstract T add(double right);

	public abstract T sub(BaseArithmeticable<T> right);
	public abstract T sub(int right);
	public abstract T sub(float right);
	public abstract T sub(double right);

	public abstract T negate();

	public abstract T mul(BaseArithmeticable<T> right);
	public abstract T mul(int right);
	public abstract T mul(float right);
	public abstract T mul(double right);

	public abstract T div(BaseArithmeticable<T> right);
	public abstract T div(int right);
	public abstract T div(float right);
	public abstract T div(double right);

	public static T operator+(BaseArithmeticable<T> left, BaseArithmeticable<T> right) {
		return left.add(right);
	}
	public static T operator+(BaseArithmeticable<T> left, int right) {
		return left.add(right);
	}
	public static T operator+(BaseArithmeticable<T> left, float right) {
		return left.add(right);
	}
	public static T operator+(BaseArithmeticable<T> left, double right) {
		return left.add(right);
	}

	public static T operator-(BaseArithmeticable<T> left, BaseArithmeticable<T> right) {
		return left.sub(right);
	}
	public static T operator-(BaseArithmeticable<T> left, int right) {
		return left.sub(right);
	}
	public static T operator-(BaseArithmeticable<T> left, float right) {
		return left.sub(right);
	}
	public static T operator-(BaseArithmeticable<T> left, double right) {
		return left.sub(right);
	}

	public static T operator-(BaseArithmeticable<T>right) {
		return right.negate();
	}

	public static T operator*(BaseArithmeticable<T> left, BaseArithmeticable<T> right) {
		return left.mul(right);
	}
	public static T operator*(BaseArithmeticable<T> left, int right) {
		return left.mul(right);							  
	}													  
	public static T operator*(BaseArithmeticable<T> left, float right) {
		return left.mul(right);
	}
	public static T operator*(BaseArithmeticable<T> left, double right) {
		return left.mul(right);
	}

	public static T operator/(BaseArithmeticable<T> left, BaseArithmeticable<T> right) {
		return left.div(right);
	}
	public static T operator/(BaseArithmeticable<T> left, int right) {
		return left.div(right);
	}
	public static T operator/(BaseArithmeticable<T> left, float right) {
		return left.div(right);
	}
	public static T operator/(BaseArithmeticable<T> left, double right) {
		return left.div(right);
	}
}*/

public abstract class Tweener<T>
{
	public bool spherical = false;
	public bool active = false;

	protected T startValue;
	protected T targetValue;
	protected abstract T changeValue { get; }

	protected float startTime;
	protected float duration;

	public Tweener( bool in_spherical )
	{
		spherical = in_spherical;
	}

	public void StartTween( T start, T target, float in_duration )
	{
		if( IsSameTarget(target) ) {
			return;
		}

		startValue = start;
		targetValue = target;
		duration = in_duration;
		startTime = Time.time;

		active = true;
	}
	protected abstract bool IsSameTarget(T target);

	public T Update( T atVal )
	{
		if( active ) {
			if( Time.time < startTime + duration ) {
				if( spherical ) {
					return SphericalTween();
				}
				return LinearTween();
			} else {
				active = false;
				return targetValue;
			}
		}

		return atVal;
	}

	public T SphericalTween()
	{
		T value;
		float time = Time.time - startTime;

		value = SphericalCalc(time);
		return value;
	}
	protected abstract T SphericalCalc(float time);

	public T LinearTween()
	{
		T value;
		float time = Time.time - startTime;

		value = LinearCalc(time);
		return value;
	}
	protected abstract T LinearCalc(float time);
}

public class DoubleTweener: Tweener<double>
{
	public DoubleTweener(bool in_spherical)
		:base(in_spherical) { }

	protected override double changeValue
	{
		get
		{
			return targetValue - startValue;
		}
	}

	protected override bool IsSameTarget(double target)
	{
		return targetValue == target;
	}
	protected override double SphericalCalc(float time)
	{
		return -changeValue/2 * (Mathf.Cos(Mathf.PI * time/duration) - 1) + startValue;
	}
	protected override double LinearCalc( float time )
	{
		return changeValue * (time/duration) + startValue;
	}
}
public class Vector3Tweener: Tweener<Vector3>
{
	public Vector3Tweener(bool in_spherical)
		:base(in_spherical) { }

	protected override Vector3 changeValue
	{
		get
		{
			return targetValue - startValue;
		}
	}

	protected override bool IsSameTarget(Vector3 target)
	{
		return targetValue == target;
	}
	protected override Vector3 SphericalCalc(float time)
	{
		return -changeValue/2 * (Mathf.Cos(Mathf.PI * time/duration) - 1) + startValue;
	}
	protected override Vector3 LinearCalc( float time )
	{
		return changeValue * (time/duration) + startValue;
	}
}
public class FloatTweener: Tweener<float>
{
	public FloatTweener(bool in_spherical)
		:base(in_spherical) { }

	protected override float changeValue
	{
		get
		{
			return targetValue - startValue;
		}
	}

	protected override bool IsSameTarget(float target)
	{
		return targetValue == target;
	}
	protected override float SphericalCalc(float time)
	{
		return -changeValue/2 * (Mathf.Cos(Mathf.PI * time/duration) - 1) + startValue;
	}
	protected override float LinearCalc( float time )
	{
		return changeValue * (time/duration) + startValue;
	}
}