using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Pins;
using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;

namespace Inlynk.TinyCLR.Drivers.SSD1306.Sample
{
	class Program
	{
		static void Main()
		{
			var i2cController = I2cController.FromName(STM32F4.I2cBus.I2c1);
			OLEDDisplayDriver screen = new OLEDDisplayDriver(i2cController);
			var graphics = Graphics.FromHdc(screen.Hdc);
			var brush = new SolidBrush(Color.White);
			var font = new Font("GHIMono8x5", 8);

			StringBuilder sb = new StringBuilder();
			while (true)
			{

				//graphics.Clear(Color.Black);
				//graphics.Flush();


				sb.AppendLine("-- LYNK BOT --");
				sb.AppendLine($"TIME: {DateTime.Now.ToString("hh:mm:ss")}");
				
				for (int i = 0; i < 5; i++)
				{
					graphics.DrawString(sb.ToString(), font, brush, 32, 0);
					//	graphics.DrawEllipse(new Pen(Color.White), 0, screen.Height / 2 - 16, 24, 24);
					if (i == 0)
						graphics.DrawEllipse(new Pen(Color.White), 2, (screen.Height / 2 - 16) + 2, 20, 20);
					if (i == 1)
						graphics.DrawEllipse(new Pen(Color.White), 4, (screen.Height / 2 - 16) + 4, 16, 16);
					if (i == 2)
						graphics.DrawEllipse(new Pen(Color.White), 6, (screen.Height / 2 - 16) + 6, 12, 12);
					if (i == 3)
						graphics.DrawEllipse(new Pen(Color.White), 8, (screen.Height / 2 - 16) + 8, 8, 8);
					if (i == 4)
						graphics.DrawEllipse(new Pen(Color.White), 10, (screen.Height / 2 - 16) + 10, 4, 4);

					graphics.Flush();
				}
				sb.Clear();

			}
		}
	}
}
