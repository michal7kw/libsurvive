import tkinter as tk
from tkinter import ttk
import subprocess

class Application(tk.Frame):
    def __init__(self, master=None):
        super().__init__(master)
        self.master = master
        self.process = None
        self.ip_address = tk.StringVar(value="127.0.0.1")
        self.is_fullscreen = True
        self.create_widgets()

    def create_widgets(self):
        # Configure style
        style = ttk.Style()
        style.configure("TFrame", background="#f0f0f0")
        style.configure("TButton", font=("Arial", 14), padding=10)
        style.configure("TLabel", font=("Arial", 12), background="#f0f0f0")
        style.configure("TEntry", font=("Arial", 12))

        # Create a frame for the exit button
        self.exit_frame = ttk.Frame(self.master)
        self.exit_frame.pack(side="top", fill="x")

        # Create the exit button
        self.exit_button = ttk.Button(self.exit_frame, text="Exit Fullscreen", command=self.toggle_fullscreen)
        self.exit_button.pack(side="right", padx=10, pady=10)

        # Create a frame for the IP address label and entry
        self.ip_frame = ttk.Frame(self.master)
        self.ip_frame.pack(fill="x", padx=20, pady=10)

        self.ip_label = ttk.Label(self.ip_frame, text="IP Address:")
        self.ip_label.pack(side="left")

        self.ip_entry = ttk.Entry(self.ip_frame, textvariable=self.ip_address)
        self.ip_entry.pack(side="left", fill="x", expand=True)

        # Create a frame for the start and stop buttons
        self.button_frame = ttk.Frame(self.master)
        self.button_frame.pack(fill="both", expand=True)

        self.start_button = ttk.Button(self.button_frame, text="Start", command=self.start_script, style="Start.TButton")
        self.start_button.pack(side="left", padx=20, pady=20, fill="both", expand=True)

        self.stop_button = ttk.Button(self.button_frame, text="Stop", command=self.stop_script, style="Stop.TButton")
        self.stop_button.pack(side="right", padx=20, pady=20, fill="both", expand=True)

        # Configure button styles
        style.configure("Start.TButton", background="green", foreground="white")
        style.configure("Stop.TButton", background="red", foreground="white")

    def start_script(self):
        if self.process is None:
            ip_address = self.ip_address.get()
            command = f"websocketd --passenv OPENBLAS_NUM_THREADS --passenv HOME --port 8080 --address {ip_address} /home/michal/Documents/Githubs/libsurvive/bin/survive-cli --record-stdout --no-record-imu --report-covariance 30"
            self.process = subprocess.Popen(command, shell=True)

    def stop_script(self):
        if self.process is not None:
            self.process.kill()
            self.process = None

    def toggle_fullscreen(self):
        self.is_fullscreen = not self.is_fullscreen
        self.master.attributes("-fullscreen", self.is_fullscreen)
        if self.is_fullscreen:
            self.exit_button.config(text="Exit Fullscreen")
        else:
            self.exit_button.config(text="Enter Fullscreen")

root = tk.Tk()
root.title("Script Controller")

# Set the window to full screen mode
root.attributes("-fullscreen", True)

app = Application(master=root)
app.pack(fill="both", expand=True)
app.mainloop()