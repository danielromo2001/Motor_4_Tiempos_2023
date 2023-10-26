using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Threading;

class Motor4TiemposSimulation : Form
{
    private double maxRPM = 6000.0; // RPM máximas
    private double maxThrottle = 1.0; // Máxima apertura del acelerador (0.0 - 1.0)
    private double throttle = 0.5; // Apertura del acelerador
    private double acceleration = 0.005; // Velocidad de aceleración
    private double deceleration = 0.01; // Velocidad de desaceleración
    private System.Threading.Timer timer;
    private double valveTimer = 0.0; // Temporizador para controlar las válvulas
    private bool isValveOpen = false; // Indica si las válvulas están abiertas
    private double engineSpeed = 0.0;
    private double engineTemperature = 80.0; // Temperatura del motor en grados Celsius
    private double fuelLevel = 50.0; // Nivel de combustible en porcentaje
    private double timeElapsed = 0.0;
    private int pistonPosition = 10;
    private int direction = 1;
    private EngineCycle currentCycle = EngineCycle.Intake;
    private bool isIntakeValveOpen = true;
    private bool isExhaustValveOpen = true;
    private bool isIgnition = false;
    private bool isSparkPlugged = false;
    private bool isOilChangeNeeded = false;

    public Motor4TiemposSimulation()
    {
        this.Size = new Size(800, 500);
        this.Text = "Simulación Motor de 4 Tiempos";
        this.DoubleBuffered = true;

        // Crea un temporizador para la simulación
        timer = new System.Threading.Timer(TimerCallback, null, 0, 1000 / 60); // 60 FPS
        // Crea un temporizador específico para controlar las válvulas
        
    }


    private void TimerCallback(object o)
    {
        timeElapsed += 1.0 / 60; // Tiempo transcurrido en segundos (60 FPS)

        // Control de aceleración y desaceleración
        if (engineSpeed >= maxRPM * throttle)
        {
            // Desaceleración
            throttle -= deceleration;
            if (throttle < 0.0)
            {
                throttle = 0.0;
            }
        }
        else
        {
            // Aceleración
            throttle += acceleration;
            if (throttle > maxThrottle)
            {
                throttle = maxThrottle;
            }
        }

        // Calcula las RPM del motor
        engineSpeed = maxRPM * throttle * Math.Sin(2 * Math.PI * timeElapsed / 60.0);

        // Simula el movimiento del pistón subiendo y bajando
        pistonPosition += direction * 2; // Ajusta la velocidad del pistón según lo desees

        if (pistonPosition >= 200)
        {
            direction = -1;
        }
        else if (pistonPosition <= 10)
        {
            direction = 1;
        }

        // Calcula la fase del motor
        int crankAngle = (int)(360.0 * pistonPosition / 200);
        int cycle = (crankAngle / 180) % 4;

        if (cycle == 0)
        {
            // Fase de admisión
            isIntakeValveOpen = true; // Abre la válvula de admisión
            // Realiza cálculos de admisión, mezcla aire y combustible, etc.
        }
        else if (cycle == 1)
        {
            // Fase de compresión
            isIntakeValveOpen = false; // Cierra la válvula de admisión
            // ... (Cálculos de compresión)
        }
        else if (cycle == 2)
        {
            // Fase de ignición
            isSparkPlugged = true; // Enciende la chispa
            // Realiza cálculos de ignición, quema del combustible, etc.
        }
        else
        {
            isSparkPlugged = false; // Apaga la chispa
            // Fase de escape
            isExhaustValveOpen = true; // Abre la válvula de escape
            // Realiza cálculos de expulsión de gases, etc.
        }

        // Simula cambios de temperatura
        engineTemperature += 0.01 * engineSpeed * engineSpeed / 3600;

        // Simula el consumo de combustible
        fuelLevel -= 0.00001 * engineSpeed;

        // Comprueba si se necesita cambio de aceite
        if (engineSpeed >= maxRPM * 0.8)
        {
            isOilChangeNeeded = true;
        }
        else
        {
            isOilChangeNeeded = false;
        }

        // Controla las fases del motor
        if (engineSpeed >= maxRPM * 0.8)
        {
            currentCycle = EngineCycle.Exhaust;
        }
        else
        {
            switch (currentCycle)
            {
                case EngineCycle.Intake:
                    if (pistonPosition < 100)
                    {
                        isIntakeValveOpen = true;
                        isExhaustValveOpen = false;
                        isIgnition = false;
                    }
                    else
                    {
                        isIntakeValveOpen = false;
                        isExhaustValveOpen = false;
                        isIgnition = false;
                        currentCycle = EngineCycle.Compression;
                    }
                    break;
                case EngineCycle.Compression:
                    if (pistonPosition < 150)
                    {
                        isIntakeValveOpen = false;
                        isExhaustValveOpen = false;
                        isIgnition = false;
                    }
                    else
                    {
                        isIntakeValveOpen = false;
                        isExhaustValveOpen = false;
                        isIgnition = true;
                        currentCycle = EngineCycle.Ignition;
                    }
                    break;
                case EngineCycle.Ignition:
                    isExhaustValveOpen = true; // Abre la válvula de escape
                    isIgnition = true;
                    currentCycle = EngineCycle.Exhaust;
                    break;
                case EngineCycle.Exhaust:
                    isExhaustValveOpen = false; // Cierra la válvula de escape
                    isIgnition = false;
                    currentCycle = EngineCycle.Intake;
                    break;
            }
        }

        this.Invalidate();
    }


    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;

        // Dibuja el cilindro
        g.FillRectangle(Brushes.Silver, 110, 50, 200, 200);

        // Dibuja el pistón como un rectángulo que sube y baja con bordes redondeados
        DrawRoundedRectangle(g, Brushes.Black, 110, 240 - pistonPosition, 200, 40, 10);

        // Dibuja las válvulas de admisión y escape
        int valveWidth = 40;
        int valveHeight = 20;
        int valveX = 130;
        int valveY = 30;
        int valveCornerRadius = 10;

        if (isIntakeValveOpen)
        {
            DrawRoundedRectangle(g, Brushes.Blue, valveX, valveY, valveWidth, valveHeight, valveCornerRadius);
        }

        valveX = 230;

        if (isExhaustValveOpen)
        {
            DrawRoundedRectangle(g, Brushes.Blue, valveX, valveY, valveWidth, valveHeight, valveCornerRadius);
        }

        // Dibuja la chispa de ignición
        if (isSparkPlugged)
        {
            g.FillEllipse(Brushes.Yellow, 200, 40, 20, 20);
        }

        // Muestra si se necesita cambio de aceite
        if (isOilChangeNeeded)
        {
            g.DrawString("Cambio de aceite necesario", new Font("Arial", 12), Brushes.Red, 350, 300);
        }

        // Dibuja la temperatura del motor
        g.DrawString($"Temperatura: {engineTemperature:F1}°C", new Font("Arial", 12), Brushes.Orange, 350, 50);

        // Dibuja el nivel de combustible
        g.DrawString($"Combustible: {fuelLevel:F1}%", new Font("Arial", 12), Brushes.Green, 350, 80);
    }

    private void DrawRoundedRectangle(Graphics g, Brush brush, int x, int y, int width, int height, int cornerRadius)
    {
        Rectangle rect = new Rectangle(x, y, width, height);
        GraphicsPath path = new GraphicsPath();
        path.AddArc(x, y, cornerRadius * 2, cornerRadius * 2, 180, 90);
        path.AddArc(x + width - 2 * cornerRadius, y, cornerRadius * 2, cornerRadius * 2, 270, 90);
        path.AddArc(x + width - 2 * cornerRadius, y + height - 2 * cornerRadius, cornerRadius * 2, cornerRadius * 2, 0, 90);
        path.AddArc(x, y + height - 2 * cornerRadius, cornerRadius * 2, cornerRadius * 2, 90, 90);
        path.CloseAllFigures();

        g.FillPath(brush, path);
    }

    static void Main()
    {
        Application.Run(new Motor4TiemposSimulation());
    }

    // Define las fases del motor
    enum EngineCycle
    {
        Intake,
        Compression,
        Ignition,
        Exhaust
    }
}

