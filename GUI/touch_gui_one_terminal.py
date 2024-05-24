#!/usr/bin/env python3

import tkinter as tk
from tkinter import ttk
from tkinter.scrolledtext import ScrolledText
import subprocess
import threading
import signal

class Application(tk.Frame):
    def __init__(self, master=None):
        super().__init__(master)
        self.master = master
        self.process = None
        self.calibrate_process = None
        # self.ip_address = ["192", "168", "8", "112"]
        self.ip_address = ["127", "0", "0", "1"]
        self.port = ["8", "0", "8", "0"]
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

        # Create a frame for the calibrate and stop calibrate buttons
        self.calibrate_frame = ttk.Frame(self.master)
        self.calibrate_frame.pack(fill="x", padx=20, pady=10)

        self.calibrate_button = ttk.Button(self.calibrate_frame, text="Calibrate", command=self.calibrate, style="Calibrate.TButton")
        self.calibrate_button.pack(side="left", fill="x", padx=20, pady=10, expand=True)

        # self.stop_calibrate_button = ttk.Button(self.calibrate_frame, text="Stop Calibrate", command=self.stop_calibrate, style="StopCalibrate.TButton")
        self.stop_calibrate_button = ttk.Button(self.calibrate_frame, text="Stop Calibrate", command=self.stop_calibrate, style="StopCalibrate.TButton", state='disabled')
        self.stop_calibrate_button.pack(side="right", fill="x", padx=20, pady=10, expand=True)

        # Configure button styles
        style.configure("Calibrate.TButton", background="blue", foreground="white", font=("Arial", 18), padding=10)
        style.configure("StopCalibrate.TButton", background="orange", foreground="white", font=("Arial", 18), padding=10)

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

        # Create a frame for the port label and entry
        self.port_frame = ttk.Frame(self.master)
        self.port_frame.pack(fill="x", padx=20, pady=10)

        self.port_label = ttk.Label(self.port_frame, text="Port:")
        self.port_label.pack(side="left")

        self.port_entries = []
        for i in range(4):
            port_entry = ttk.Entry(self.port_frame, width=3, font=("Arial", 24), justify="center")
            port_entry.insert(0, self.port[i])
            port_entry.pack(side="left", padx=5)
            self.port_entries.append(port_entry)

            up_button = ttk.Button(self.port_frame, text="▲", width=2, command=lambda idx=i: self.increment_port_digit(idx))
            up_button.pack(side="left")

            down_button = ttk.Button(self.port_frame, text="▼", width=2, command=lambda idx=i: self.decrement_port_digit(idx))
            down_button.pack(side="left")

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
            # self.release_usb_interface()
            ip_address = ".".join(self.ip_address)
            port = "".join(self.port)
            command = f"websocketd --passenv OPENBLAS_NUM_THREADS --passenv HOME --port {port} --address {ip_address} /home/michal/Documents/libsurvive/bin/survive-cli --record-stdout --no-record-imu --report-covariance 30"
            self.process = subprocess.Popen(command, shell=True, stdout=subprocess.PIPE, stderr=subprocess.PIPE, universal_newlines=True)
            threading.Thread(target=self.monitor_process).start()

    def monitor_process(self):
        while self.process:
            output = self.process.stdout.readline().strip()
            if output:
                self.terminal_display.insert(tk.END, output + "\n")
                self.terminal_display.see(tk.END)

    def stop_script(self):
        if self.process is not None:
            self.process.kill()
            self.process = None
            
            port = "".join(self.port)
            command = f"lsof -t -i:{port} | xargs kill -9"
            subprocess.run(command, shell=True)

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

    def increment_port_digit(self, idx):
        digit = int(self.port[idx])
        if digit < 9:
            self.port[idx] = str(digit + 1)
            self.port_entries[idx].delete(0, tk.END)
            self.port_entries[idx].insert(0, self.port[idx])

    def decrement_port_digit(self, idx):
        digit = int(self.port[idx])
        if digit > 0:
            self.port[idx] = str(digit - 1)
            self.port_entries[idx].delete(0, tk.END)
            self.port_entries[idx].insert(0, self.port[idx])

    def calibrate(self):
        if self.calibrate_process is None:            
            self.terminal_display.configure(state='normal')
            self.terminal_display.delete('1.0', tk.END)
            # self.terminal_display.configure(state='disabled')
            
            command = "/home/michal/Documents/libsurvive/bin/survive-cli --force-calibrate"
            self.calibrate_process = subprocess.Popen(command, shell=True, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, universal_newlines=True)
            threading.Thread(target=self.monitor_calibrate_process).start()
            self.calibrate_button.configure(state='disabled')
            self.stop_calibrate_button.configure(state='normal')

    def stop_calibrate(self):
        if self.calibrate_process is not None:
            self.calibrate_process.send_signal(signal.SIGKILL)
            self.calibrate_process.wait()
            self.calibrate_process = None
            self.calibrate_button.configure(state='normal')
            # self.stop_calibrate_button.configure(state='disabled')

    def monitor_calibrate_process(self):
        while self.calibrate_process is not None and self.calibrate_process.poll() is None:
            output = self.calibrate_process.stdout.readline()
            if output:
                self.terminal_display.configure(state='normal')
                self.terminal_display.insert(tk.END, output)
                self.terminal_display.see(tk.END)
                # self.terminal_display.configure(state='disabled')
        
        if self.calibrate_process is not None:
            self.calibrate_process = None
            self.calibrate_button.configure(state='normal')
            # self.stop_calibrate_button.configure(state='disabled')

root = tk.Tk()
root.title("Script Controller")

# Set the window to full screen mode
root.attributes("-fullscreen", True)

app = Application(master=root)
app.pack(fill="both", expand=True)
app.mainloop()
