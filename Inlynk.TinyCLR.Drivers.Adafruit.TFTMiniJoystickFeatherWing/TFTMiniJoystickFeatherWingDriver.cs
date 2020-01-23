using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Devices.Spi;
using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Inlynk.TinyCLR.Drivers.Adafruit.TFTMiniJoystickFeatherWing
{
	public enum TFTWING_BUTTON : int
	{
		UP = 1 << 2,
		LEFT = 1 << 3,
		DOWN = 1 << 4,
		RIGHT = 1 << 7,
		RESET = 1 << 8,
		B = 1 << 9,
		A = 1 << 10,
		SELECT = 1 << 11,
	}

	public enum TFTWING_BUTTON_MODE
	{
		OUTPUT,
		INPUT,
		INPUT_PULLUP,
		INPUT_PULLDOWN
	}
	public delegate void TFTWINGBUTTONStateChangedHandler(TFTWING_BUTTON button, bool state);
	public class TFTMiniJoystickFeatherWingDriver : IDisposable
	{
		private GpioPin _dataCommandPin;
		private SpiDevice _spiDevice;
		private I2cDevice _seeSaw;
		private int TFTWING_BUTTON_ALL = (int)(TFTWING_BUTTON.UP | TFTWING_BUTTON.DOWN | TFTWING_BUTTON.LEFT | TFTWING_BUTTON.RIGHT | TFTWING_BUTTON.SELECT | TFTWING_BUTTON.A | TFTWING_BUTTON.B);
		private byte[] _Command = new byte[6];
		private byte SEESAW_GPIO_BASE = 0x01;
		private byte SEESAW_GPIO_DIRSET_BULK = 0x02;
		private byte SEESAW_GPIO_DIRCLR_BULK = 0x03;
		private byte SEESAW_GPIO_BULK_SET = 0x05;
		private byte SEESAW_GPIO_BULK_CLR = 0x06;
		private byte SEESAW_GPIO_PULLENSET = 0x0B;
		private byte SEESAW_GPIO_BULK = 0x04;
		private bool _isRunning;
		private int _pinsChanged = 0;

		public event TFTWINGBUTTONStateChangedHandler OnButtonPressed = null;
		public TFTMiniJoystickFeatherWingDriver(SpiDevice spiDevice, GpioPin dataCommandPin, I2cController i2cController)
		{
			_dataCommandPin = dataCommandPin;
			_dataCommandPin.SetDriveMode(GpioPinDriveMode.Output);
			_dataCommandPin.Write(GpioPinValue.Low);
			_spiDevice = spiDevice;
			_seeSaw = i2cController.GetDevice(new I2cConnectionSettings(0x5E) { BusSpeed = I2cBusSpeed.FastMode, AddressFormat = I2cAddressFormat.SevenBit });
			_isRunning = false;

			Init();
		}

		private void Init()
		{
			_isRunning = true;
			SetPinMode(TFTWING_BUTTON.RESET, TFTWING_BUTTON_MODE.OUTPUT);
			SetPinMode((TFTWING_BUTTON)TFTWING_BUTTON_ALL, TFTWING_BUTTON_MODE.INPUT_PULLUP);
			new Thread(() =>
			{

				while (_isRunning)
				{
					var buffer = new byte[4];
					_seeSaw.WriteRead(new byte[] { SEESAW_GPIO_BASE, SEESAW_GPIO_BULK }, buffer);
					int val = ((buffer[0] << 24) | (buffer[1] << 16) | (buffer[2] << 8) | buffer[3]) & TFTWING_BUTTON_ALL;
					int changed = TFTWING_BUTTON_ALL - val;
					if (changed != _pinsChanged)
					{
						RunNotifyButtonPressed(changed);
					}
					Thread.Sleep(10);
				}
			}).Start();
		}

		private void RunNotifyButtonPressed(int pins)
		{
			bool isAdded = true;
			var changed = pins - _pinsChanged;
			if (changed < 0)
			{
				isAdded = false;
				changed *= -1;
			}

			for (int i = 16; i > 0; i--)
			{
				var pin = 1 << i;
				if (changed >= pin)
				{
					OnButtonPressed?.Invoke((TFTWING_BUTTON)pin, isAdded);
					changed -= pin;
				}
			}
			_pinsChanged = pins;
		}

		public void Dispose()
		{
			_isRunning = false;
			_seeSaw.Dispose();
			_spiDevice.Dispose();
		}

		public void SetPinMode(TFTWING_BUTTON pins, TFTWING_BUTTON_MODE mode)
		{
			_Command[0] = SEESAW_GPIO_BASE;
			byte[] buffer = { (byte)((int)pins >> 24), (byte)((int)pins >> 16), (byte)((int)pins >> 8), (byte)pins };
			Array.Copy(buffer, 0, _Command, 2, buffer.Length);
			switch (mode)
			{
				case TFTWING_BUTTON_MODE.OUTPUT:
					_Command[1] = SEESAW_GPIO_DIRSET_BULK;
					_seeSaw.Write(_Command);
					break;
				case TFTWING_BUTTON_MODE.INPUT:
					_Command[1] = SEESAW_GPIO_DIRCLR_BULK;
					_seeSaw.Write(_Command);

					break;
				case TFTWING_BUTTON_MODE.INPUT_PULLUP:
					_Command[1] = SEESAW_GPIO_DIRCLR_BULK;
					_seeSaw.Write(_Command);
					_Command[1] = SEESAW_GPIO_PULLENSET;
					_seeSaw.Write(_Command);
					_Command[1] = SEESAW_GPIO_BULK_SET;
					_seeSaw.Write(_Command);
					break;
				case TFTWING_BUTTON_MODE.INPUT_PULLDOWN:
					_Command[1] = SEESAW_GPIO_DIRCLR_BULK;
					_seeSaw.Write(_Command);
					_Command[1] = SEESAW_GPIO_PULLENSET;
					_seeSaw.Write(_Command);
					_Command[1] = SEESAW_GPIO_BULK_CLR;
					_seeSaw.Write(_Command);
					break;
			}
		}
	}


}
