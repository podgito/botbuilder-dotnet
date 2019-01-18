﻿//// Copyright (c) Microsoft Corporation. All rights reserved.
//// Licensed under the MIT License.

//using System;
//using System.Collections.Generic;
//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.Bot.Builder.Dialogs;

//namespace Microsoft.Bot.Builder.Dialogs.Composition
//{
//    public class ComposableDialog : ComponentDialog, IComposableDialog
//    {
//        protected const string PersistedDialogState = "ComposableDialog";
//        private DialogSet dialogSet;

//        public ComposableDialog(string dialogId=null)
//            : base(dialogId)
//        {
//        }

//        /// <summary>
//        /// Gets or sets or set the <see cref="IBotTelemetryClient"/> to use.
//        /// When setting this property, all the contained dialogs TelemetryClient properties are also set.
//        /// </summary>
//        /// <value>The <see cref="IBotTelemetryClient"/> to use when logging.</value>
//        public new IBotTelemetryClient TelemetryClient
//        {
//            get
//            {
//                return base.TelemetryClient;
//            }

//            set
//            {
//                base.TelemetryClient = value ?? NullBotTelemetryClient.Instance;
//            }
//        }

//        /// <summary>
//        /// Dialogs 
//        /// </summary>
//        public IDictionary<string, IDialog> Dialogs { get; set; } = new Dictionary<string, IDialog>();

//        /// <summary>
//        /// Interruption Dialog
//        /// </summary>
//        public IDialog InterruptionDialog { get; set; } 

//        /// <summary>
//        /// Fallback Dialog
//        /// </summary>
//        public IDialog FallbackDialog { get; set; }

//        /// <summary>
//        /// Slot definitions
//        /// </summary>
//        public IDictionary<string, ISlot> Slots { get; set; } = new Dictionary<string, ISlot>();
        
//        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext outerDc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
//        {
//            this.I
//            if (outerDc == null)
//            {
//                throw new ArgumentNullException(nameof(outerDc));
//            }

//            // Start the inner dialog.
//            var dialogState = new DialogState();
//            outerDc.ActiveDialog.State[PersistedDialogState] = dialogState;

//            var innerDc = new DialogContext(getDialogSet(), outerDc.Context, dialogState);
//            var turnResult = await OnBeginDialogAsync(innerDc, options, cancellationToken).ConfigureAwait(false);

//            // Check for end of inner dialog
//            if (turnResult.Status != DialogTurnStatus.Waiting)
//            {
//                // Return result to calling dialog
//                return await EndComponentAsync(outerDc, turnResult.Result, cancellationToken).ConfigureAwait(false);
//            }
//            else
//            {
//                // Just signal waiting
//                return Dialog.EndOfTurn;
//            }
//        }

//        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext outerDc, CancellationToken cancellationToken = default(CancellationToken))
//        {
//            if (outerDc == null)
//            {
//                throw new ArgumentNullException(nameof(outerDc));
//            }

//            // Continue execution of inner dialog.
//            var dialogState = (DialogState)outerDc.ActiveDialog.State[PersistedDialogState];
//            var innerDc = new DialogContext(_dialogs, outerDc.Context, dialogState);
//            var turnResult = await OnContinueDialogAsync(innerDc, cancellationToken).ConfigureAwait(false);

//            if (turnResult.Status != DialogTurnStatus.Waiting)
//            {
//                return await EndComponentAsync(outerDc, turnResult.Result, cancellationToken).ConfigureAwait(false);
//            }
//            else
//            {
//                return Dialog.EndOfTurn;
//            }
//        }

//        public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext outerDc, DialogReason reason, object result = null, CancellationToken cancellationToken = default(CancellationToken))
//        {
//            // Containers are typically leaf nodes on the stack but the dev is free to push other dialogs
//            // on top of the stack which will result in the container receiving an unexpected call to
//            // dialogResume() when the pushed on dialog ends.
//            // To avoid the container prematurely ending we need to implement this method and simply
//            // ask our inner dialog stack to re-prompt.
//            await RepromptDialogAsync(outerDc.Context, outerDc.ActiveDialog, cancellationToken).ConfigureAwait(false);
//            return Dialog.EndOfTurn;
//        }

//        public override async Task RepromptDialogAsync(ITurnContext turnContext, DialogInstance instance, CancellationToken cancellationToken = default(CancellationToken))
//        {
//            // Delegate to inner dialog.
//            var dialogState = (DialogState)instance.State[PersistedDialogState];
//            var innerDc = new DialogContext(_dialogs, turnContext, dialogState);
//            await innerDc.RepromptDialogAsync(cancellationToken).ConfigureAwait(false);

//            // Notify component
//            await OnRepromptDialogAsync(turnContext, instance, cancellationToken).ConfigureAwait(false);
//        }

//        public override async Task EndDialogAsync(ITurnContext turnContext, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default(CancellationToken))
//        {
//            // Forward cancel to inner dialogs
//            if (reason == DialogReason.CancelCalled)
//            {
//                var dialogState = (DialogState)instance.State[PersistedDialogState];
//                var innerDc = new DialogContext(_dialogs, turnContext, dialogState);
//                await innerDc.CancelAllDialogsAsync(cancellationToken).ConfigureAwait(false);
//            }

//            await OnEndDialogAsync(turnContext, instance, reason, cancellationToken).ConfigureAwait(false);
//        }

//        public ComposableDialog AddDialog(Dialog dialog)
//        {
//            return AddDialog(dialog as IDialog);
//        }

//        /// <summary>
//        /// Adds a dialog to the component dialog.
//        /// </summary>
//        /// <param name="dialog">The dialog to add.</param>
//        /// <returns>The updated <see cref="ComposableDialog"/>.</returns>
//        /// <remarks>Adding a new dialog will inherit the <see cref="IBotTelemetryClient"/> of the ComponentDialog.</remarks>
//        public ComposableDialog AddDialog(IDialog dialog)
//        {
//            if (string.IsNullOrEmpty(dialog.Id))
//            {
//                throw new ArgumentNullException("Dialog.Id", "You can't add a dialog without an Id set.");
//            }

//            _dialogs.Add(dialog);
//            if (string.IsNullOrEmpty(InitialDialogId))
//            {
//                InitialDialogId = dialog.Id;
//            }

//            return this;
//        }

//        /// <summary>
//        /// Finds a dialog by ID.
//        /// </summary>
//        /// <param name="dialogId">The ID of the dialog to find.</param>
//        /// <returns>The dialog; or <c>null</c> if there is not a match for the ID.</returns>
//        public IDialog FindDialog(string dialogId)
//        {
//            return _dialogs.Find(dialogId);
//        }

//        protected virtual Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = default(CancellationToken))
//        {
//            return innerDc.BeginDialogAsync(InitialDialogId, options, cancellationToken);
//        }

//        protected virtual Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken))
//        {
//            return innerDc.ContinueDialogAsync(cancellationToken);
//        }

//        protected virtual Task OnEndDialogAsync(ITurnContext context, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default(CancellationToken))
//        {
//            return Task.CompletedTask;
//        }

//        protected virtual Task OnRepromptDialogAsync(ITurnContext turnContext, DialogInstance instance, CancellationToken cancellationToken = default(CancellationToken))
//        {
//            return Task.CompletedTask;
//        }

//        protected virtual Task<DialogTurnResult> EndComponentAsync(DialogContext outerDc, object result, CancellationToken cancellationToken)
//        {
//            return outerDc.EndDialogAsync(result);
//        }

//        private DialogSet getDialogSet()
//        {
//            if (this.dialogSet == null)
//            {
//                this.dialogSet = new DialogSet();
//                foreach (var dialog in this.Dialogs.Values)
//                {
//                    dialogSet.Add(dialog);
//                }
//            }
//            return this.dialogSet;
//        }
//    }
//}
