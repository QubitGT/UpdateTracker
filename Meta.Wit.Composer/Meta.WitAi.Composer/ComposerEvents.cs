using System;
using UnityEngine;

namespace Meta.WitAi.Composer;

[Serializable]
public class ComposerEvents
{
	[Header("Session Events")]
	public ComposerSessionEvent OnComposerSessionBegin;

	public ComposerSessionEvent OnComposerSessionEnd;

	[Header("Setup Events")]
	public ComposerActiveEvent OnComposerActiveChange;

	public ComposerSessionEvent OnComposerContextMapChange;

	public ComposerSessionEvent OnComposerActivation;

	public ComposerSessionEvent OnComposerRequestInit;

	[Header("Request Events")]
	public ComposerSessionRequestEvent OnComposerRequestCreated;

	public ComposerSessionRequestEvent OnComposerRequestSetup;

	[Header("Response Events")]
	public ComposerSessionEvent OnComposerRequestBegin;

	public ComposerSessionEvent OnComposerResponse;

	public ComposerSessionEvent OnComposerError;

	public ComposerSessionEvent OnComposerCanceled;

	[Header("Handler Events")]
	public ComposerSessionEvent OnComposerExpectsInput;

	public ComposerSessionEvent OnComposerSpeakPhrase;

	public ComposerSessionEvent OnComposerPerformAction;

	public ComposerSessionEvent OnComposerComplete;
}
