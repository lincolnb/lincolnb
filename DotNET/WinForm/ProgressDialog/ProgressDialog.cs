using System;
using System.Windows.Forms;

namespace Websolutions
{
    /** 
    <summary>
        Implements a dialog to spawn a thread to execute some long-running code.
    </summary>
    <remarks>
        Demonstrates safe cross-thread invocation and stuff.
    </remarks>
    */
    public sealed class ProgressDialog : Form
    {
        public delegate object ProgressDialogStart ( params object[] Params );

        private ProgressDialogStart onstart = null;
        private object[] paramlist = null;
        private object result = null;
        private bool Configured = false;

        public ProgressDialog()
        {
        }

        /** 
        <summary>
            Constructor.
        </summary>
        <remarks>
            Uses the default icon.
        </remarks>
        <param name="Text">
            Caption for the dialog.
        </param>
        <param name="Style">
            Style for the ProgressBar.
        </param>
        <param name="Cancelable">
            Whether or not to show the Cancel button.
        </param>
        <param name="StartMethod">
            The method to run.
        </param>
        <param name="Params">
            Parameters to pass to the StartMethod when it runs (optional).
        </param>
        <exception cref="System.NullReferenceException">
            Will be thrown if the StartMethod value is null.
        </exception>
        */
        public ProgressDialog
        (
            string Text,
            ProgressBarStyle Style,
            bool Cancelable,
            ProgressDialogStart StartMethod,
            params object[] Params
        ) : this ( Text , null , Style , Cancelable , StartMethod , Params )
        {
        }

        /** 
        <summary>
            Constructor.
        </summary>
        <remarks>
        </remarks>
        <param name="Text">
            Caption for the dialog.
        </param>
        <param name="Style">
            Style for the ProgressBar.
        </param>
        <param name="Cancelable">
            Whether or not to show the Cancel button.
        </param>
        <param name="StartMethod">
            The method to run.
        </param>
        <param name="Params">
            Parameters to pass to the StartMethod when it runs (optional).
        </param>
        <exception cref="System.NullReferenceException">
            Will be thrown if the StartMethod value is null.
        </exception>
        */
        public ProgressDialog
        (
            string Text,
            System.Drawing.Icon Icon,
            ProgressBarStyle Style,
            bool Cancelable,
            ProgressDialogStart StartMethod,
            params object[] Params
        ) : base ()
        {
            Configure(Text, Icon, Style, Cancelable, StartMethod, Params);
        }

        /** 
        <summary>
            Configure, required when the unparameterized constructor has been called
        </summary>
        <remarks>
        </remarks>
        <param name="Text">
            Caption for the dialog.
        </param>
        <param name="Style">
            Style for the ProgressBar.
        </param>
        <param name="Cancelable">
            Whether or not to show the Cancel button.
        </param>
        <param name="StartMethod">
            The method to run.
        </param>
        <param name="Params">
            Parameters to pass to the StartMethod when it runs (optional).
        </param>
        <exception cref="System.NullReferenceException">
            Will be thrown if the StartMethod value is null.
        </exception>
        */
        public void Configure 
        (
            string Text,
            System.Drawing.Icon Icon,
            ProgressBarStyle Style,
            bool Cancelable,
            ProgressDialogStart StartMethod,
            params object[] Params
        )
        {
            if ( StartMethod != null )
            {
                InitializeComponent();
                
                this.Text = Text;
                if ( Icon != null )
                {
                    this.Icon = Icon;
                }
                this.pbProgress.Style = Style;
                this.bCancel.Visible = Cancelable;
                if (!Cancelable) this.AutoSize = true;
                this.onstart = StartMethod;
                this.paramlist = Params;  
                this.OnUpdateProgress += new UpdateProgress ( this.UpdateProgressHandler );

                Configured = true;
            }
            else
            {
                throw ( new System.NullReferenceException ( "A start method must be provided" ) );
            }
        }

        /** 
        <summary>
            Shows the dialog and runs the method.
        </summary>
        <remarks>
        </remarks>
        <returns>
            DialogResult.OK or DialogResult.Cancel
        </returns>
        */
        public new DialogResult ShowDialog()
        {
            if (!Configured) throw new Exception("The Progress dialog has not been configured. Please instantiated with one of the parameterized constructors or call Configure() prior to ShowDialog.");
    
            base.ShowDialog();

            return ( (DialogResult) this.bCancel.Tag );
        }

#region Public properties

        /** 
        <summary>
            Retrieves the result of the method.
        </summary>
        <remarks>
        </remarks>
        <returns>
            The result of the method.
        </returns>
        */
        public object Result
        {
            get
            {
                return ( this.result );
            }
        }

        private delegate bool CancelGetDelegate();

        /** 
        <summary>
            Whether or not cancel was requested.
        </summary>
        <remarks>
        </remarks>
        <returns>
            True if a cancel was requested, otherwise true.
        </returns>
        */
        public bool WasCancelled
        {
            get
            {
                if ( InvokeRequired )
                {
                    CancelGetDelegate temp = delegate ()
                    {
                        return ( this.WasCancelled );
                    };

                    return ( (bool) this.Invoke ( temp ) );
                }
                else
                {
                    return 
                    ( 
                        (DialogResult) this.bCancel.Tag == DialogResult.Cancel 
                    );
                }
            }
        }
#endregion
        
#region Private properties

        private delegate DialogResult StateGetDelegate();
        private delegate void StateSetDelegate ( DialogResult State );
        
        private DialogResult State
        {
            get
            {
                if ( InvokeRequired )
                {
                    StateGetDelegate temp = delegate()
                    {
                        return ( this.State );
                    };
                    
                    return ( (DialogResult) this.Invoke ( temp ) ); 
                }
                else
                {
                    return ( this.DialogResult );
                }
            }

            set
            {
                if ( InvokeRequired )
                {
                    StateSetDelegate temp = delegate ( DialogResult State )
                    {
                        this.State = value;
                    };
                    
                    this.Invoke ( temp , value );
                }
                else
                {
                    this.DialogResult = value;
                }
            }
        }

        private delegate int ProgressGetDelegate();
        private delegate void ProgressSetDelegate ( int Progress );

        private int Progress
        {
            get
            {
                if ( InvokeRequired )
                {
                    ProgressGetDelegate temp = delegate()
                    {
                        return ( this.pbProgress.Value );
                    };

                    return ( (int) this.Invoke ( temp ) );
                }
                else
                {
                    return ( this.pbProgress.Value );
                }
            }

            set
            {
                if ( InvokeRequired )
                {
                    ProgressSetDelegate temp = delegate ( int Progress )
                    {
                        this.pbProgress.Value = value;
                    };

                    this.Invoke ( temp , value );
                }
                else
                {
                    this.pbProgress.Value = System.Math.Abs ( value ) % this.pbProgress.Maximum+1;
                }
            }
        }

#endregion

#region Private event handlers

        private void ProgressDialog_Load(object sender, EventArgs e)
        {
            this.bCancel.Tag = DialogResult.OK;
            this.bCancel.Enabled = true;
            this.bCancel.Text = "Cancel";
            
            this.pbProgress.Value = this.pbProgress.Minimum;
            
            ( new System.Threading.Thread
            (
                delegate()
                {
                    this.result = this.onstart ( this.paramlist );

                    this.State = DialogResult.OK;
                }
            )).Start();
        }

        private void ProgressDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            switch ( this.State )
            {
                case DialogResult.None :
                {
                    e.Cancel = true;
                    break;
                }

                case DialogResult.Cancel :
                {
                    if ( !this.bCancel.Visible )
                    {
                        MessageBox.Show
                        (
                            "This operation can't be cancelled"
                        ,
                            "Can't close"
                        , 
                            MessageBoxButtons.OK
                        ,
                            MessageBoxIcon.Hand
                        );
                    }
                    else
                    {
                        this.bCancel.Enabled = false;
                        this.bCancel.Text = "Cancelling";
                        this.bCancel.Tag = this.State;
                    }

                    e.Cancel = true;
                    break;
                }
            }
        }
#endregion

# region UpdateProgress event stuff

        private event UpdateProgress OnUpdateProgress;

        /** 
        <summary>
            Set the ProgressBar to some percentage done.
        </summary>
        <remarks>
            Used for Block and Continuous style ProgressBars
        </remarks>
        <param name="Percent">
            Specify 0 through 100.
        </param>
       */
        public void
        RaiseUpdateProgress
        (
            int Percent
        )
        {
            OnUpdateProgress ( this , new UpdateProgressEventArgs ( Percent ) );
        }
        
        private void
        UpdateProgressHandler
        (
            object                  sender
        ,
            UpdateProgressEventArgs e
        )
        {
            this.Progress = e.Percent;
        }

# endregion

#region UpdateProgressEventArgs

        private delegate void UpdateProgress ( object sender , UpdateProgressEventArgs e );
        private sealed class UpdateProgressEventArgs : System.EventArgs
        {
            private int percent;
            
            public UpdateProgressEventArgs
            (
                int Percent
            )
            {
                this.percent = Percent;
            }
            
            public int Percent
            {
                get
                {
                    return ( this.percent );
                }
            }
            
            public override string ToString()
            {
                return ( this.percent.ToString() );
            }
        }
#endregion

#region Windows Form Designer generated code

        private System.ComponentModel.IContainer components = null;

        protected override void Dispose ( bool disposing )
        {
            if ( disposing && ( components != null ) )
            {
                components.Dispose();
            }
            base.Dispose ( disposing );
        }

        private void InitializeComponent ()
        {
            this.pbProgress = new System.Windows.Forms.ProgressBar();
            this.bCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // pbProgress
            // 
            this.pbProgress.Dock = System.Windows.Forms.DockStyle.Top;
            this.pbProgress.Location = new System.Drawing.Point(0, 0);
            this.pbProgress.Name = "pbProgress";
            this.pbProgress.Size = new System.Drawing.Size(294, 23);
            this.pbProgress.TabIndex = 0;
            // 
            // bCancel
            // 
            this.bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.bCancel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bCancel.Enabled = false;
            this.bCancel.Location = new System.Drawing.Point(0, 22);
            this.bCancel.Name = "bCancel";
            this.bCancel.Size = new System.Drawing.Size(294, 23);
            this.bCancel.TabIndex = 1;
            this.bCancel.Text = "Cancel";
            this.bCancel.UseVisualStyleBackColor = true;
            // 
            // ProgressDialog
            // 
            this.AcceptButton = this.bCancel;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.bCancel;
            this.ClientSize = new System.Drawing.Size(294, 45);
            this.Controls.Add(this.bCancel);
            this.Controls.Add(this.pbProgress);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProgressDialog";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ProgressDialog";
            this.Load += new System.EventHandler(this.ProgressDialog_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ProgressDialog_FormClosing);
            this.ResumeLayout(false);

        }

        private ProgressBar pbProgress;
        private Button bCancel;

# endregion

    }
}