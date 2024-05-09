import tkinter as tk
from tkinter import ttk
from tkinter.scrolledtext import ScrolledText
import subprocess

class Application(tk.Frame):
    def __init__(self, master=None):
        super().__init__(master)
        self.master = master
        self.process = None
        self.ip_address = ["127", "0", "0", "1"]
        self.is_fullscreen = True
        self.create_widgets()

    def create_widgets(self):
        # Configure style
        style = ttk.Style()
        style.configure("TFrame", background="#f0f0f0")
        style.configure("TButton", font=("Arial", 18), padding=20)
        style.configure("TLabel", font=("Arial", 16), background="#f0f0f0")
        style.configure("TEntry", font=("Arial", 20))

        # Create a frame for the exit button
        self.exit_frame = ttk.Frame(self.master)
        self.exit_frame.pack(side="top", fill="x")

        # Create the exit button
        self.exit_button = ttk.Button(self.exit_frame, text="Exit Fullscreen", command=self.toggle_fullscreen)
        self.exit_button.pack(side="right", padx=10, pady=10)

        # Create a frame for the IP address label and entry
        self.ip_frame = ttk.Frame(self.master)
        self.ip_frame.pack(fill="x", padx=20, pady=20)

        self.ip_label = ttk.Label(self.ip_frame, text="IP Address:")
        self.ip_label.pack(side="left")

        self.ip_entries = []
        for i in range(4):
            ip_entry = ttk.Entry(self.ip_frame, width=3, font=("Arial", 24), justify="center")
            ip_entry.insert(0, self.ip_address[i])
            ip_entry.pack(side="left", padx=5)
            self.ip_entries.append(ip_entry)

            up_button = ttk.Button(self.ip_frame, text="▲", width=2, command=lambda idx=i: self.increment_ip_digit(idx))
            up_button.pack(side="left")

            down_button = ttk.Button(self.ip_frame, text="▼", width=2, command=lambda idx=i: self.decrement_ip_digit(idx))
            down_button.pack(side="left")

            if i < 3:
                separator = ttk.Label(self.ip_frame, text=".")
                separator.pack(side="left", padx=5)

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

        # Create a frame for the terminal display
        self.terminal_frame = ttk.Frame(self.master)
        self.terminal_frame.pack(fill="both", padx=20, pady=20, expand=True)

        self.terminal_display = ScrolledText(self.terminal_frame, font=("Courier", 12), wrap=tk.WORD)
        self.terminal_display.pack(fill="both", expand=True)

    def start_script(self):
        if self.process is None:
            ip_address = ".".join(self.ip_address)
            command = f"websocketd --passenv OPENBLAS_NUM_THREADS --passenv HOME --port 8080 --address {ip_address} /home/michal/Documents/libsurvive/bin/survive-cli --record-stdout --no-record-imu --report-covariance 30"
            self.process = subprocess.Popen(command, shell=True, stdout=subprocess.PIPE, stderr=subprocess.PIPE, universal_newlines=True)
            self.monitor_process()

    def monitor_process(self):
        if self.process:
            output = self.process.stdout.readline().strip()
            if output:
                self.terminal_display.insert(tk.END, output + "\n")
                self.terminal_display.see(tk.END)
            self.master.after(100, self.monitor_process)

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

    def increment_ip_digit(self, idx):
        digit = int(self.ip_address[idx])
        if digit < 255:
            self.ip_address[idx] = str(digit + 1)
            self.ip_entries[idx].delete(0, tk.END)
            self.ip_entries[idx].insert(0, self.ip_address[idx])

    def decrement_ip_digit(self, idx):
        digit = int(self.ip_address[idx])
        if digit > 0:
            self.ip_address[idx] = str(digit - 1)
            self.ip_entries[idx].delete(0, tk.END)
            self.ip_entries[idx].insert(0, self.ip_address[idx])

root = tk.Tk()
root.title("Script Controller")

# Set the window to full screen mode
root.attributes("-fullscreen", True)

app = Application(master=root)
app.pack(fill="both", expand=True)
app.mainloop()
