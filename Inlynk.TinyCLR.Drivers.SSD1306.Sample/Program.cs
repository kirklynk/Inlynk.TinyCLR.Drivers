using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Pins;
using Inlynk.TinyCLR.Drivers.Adafruit.TFTMiniJoystickFeatherWing;
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading;

namespace Inlynk.TinyCLR.Drivers.SSD1306.Sample
{
	class Program
	{
		static void Main()
		{
			var gpioController = GpioController.GetDefault();
			var i2cController = I2cController.FromName(STM32F4.I2cBus.I2c1);
			var spiController = SpiController.FromName(STM32F4.SpiBus.Spi2);
			var device = spiController.GetDevice(new SpiConnectionSettings { ChipSelectType= SpiChipSelectType.Gpio, ChipSelectLine = STM32F4.GpioPin.PC7, Mode = SpiMode.Mode0, ChipSelectActiveState = false });

			var pinMode = gpioController.OpenPin(STM32F4.GpioPin.PC6);
			TFTMiniJoystickFeatherWingDriver miniJoystickFeatherWingDriver = new TFTMiniJoystickFeatherWingDriver(device, pinMode, i2cController);
			miniJoystickFeatherWingDriver.OnButtonPressed += (t, state) => {
				Debug.WriteLine($"{t} - {state}");
			};
			Thread.Sleep(-1);
			//OLEDDisplayDriver screen = new OLEDDisplayDriver(i2cController);
			//var graphics = Graphics.FromHdc(screen.Hdc);
			//var brush = new SolidBrush(Color.White);
			//var font = new Font("GHIMono8x5", 8);

			//StringBuilder sb = new StringBuilder();
			//while (true)
			//{

			//	//graphics.Clear(Color.Black);
			//	//graphics.Flush();


			//	sb.AppendLine("-- LYNK BOT --");
			//	sb.AppendLine($"TIME: {DateTime.Now.ToString("hh:mm")}");

			//	for (int i = 0; i < 5; i++)
			//	{
			//		graphics.DrawString(sb.ToString(), font, brush, 36, 8);
			//		graphics.DrawEllipse(new Pen(Color.White), 0, screen.Height / 2 - 8, 24, 24);
			//		if (i == 0)
			//			graphics.DrawEllipse(new Pen(Color.White), 2, (screen.Height / 2 - 8) + 2, 20, 20);
			//		if (i == 1)
			//			graphics.DrawEllipse(new Pen(Color.White), 4, (screen.Height / 2 - 8) + 4, 16, 16);
			//		if (i == 2)
			//			graphics.DrawEllipse(new Pen(Color.White), 6, (screen.Height / 2 - 8) + 6, 12, 12);
			//		if (i == 3)
			//			graphics.DrawEllipse(new Pen(Color.White), 8, (screen.Height / 2 - 8) + 8, 8, 8);
			//		if (i == 4)
			//			graphics.DrawEllipse(new Pen(Color.White), 10, (screen.Height / 2 - 8) + 10, 4, 4);

			//		graphics.Flush();
			//	}
			//	sb.Clear();

			//}
		}
	}
}
