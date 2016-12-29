package aurora.lights;

import be.pixxis.lpd8806.LedStrip;
import com.google.gson.Gson;
import com.pi4j.wiringpi.Spi;
import com.sun.net.httpserver.HttpExchange;
import com.sun.net.httpserver.HttpHandler;
import com.sun.net.httpserver.HttpServer;

import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.InetSocketAddress;
import java.util.concurrent.CountDownLatch;

public class AuroraPiLighting {

    //!!!!!!!!!! SETTINGS !!!!!!!!!!//
    private final static int NUMBER_OF_LEDS = 32; //Number of LEDs connected to your Raspberry pi
    private final static int USED_PORT = 8032; //Port to be used
    private final static String PI_URL_SUFFIX_START = "start";
    private final static String PI_URL_SUFFIX_STOP = "stop";
    private final static String PI_URL_SUFFIX_QUIT = "quit";
    private final static String PI_URL_SUFFIX_SETLIGHTS = "lights";


    private final static byte empty_arr[] = new byte[0];
    private static LedStrip ledStrip;
    private static boolean isInitialized = false;

    public static void main(String[] args) {

        // setup SPI for communication with the led strip.
        int fd = Spi.wiringPiSPISetup(0, 10000000);
        if (fd <= -1) {
            System.out.println("SPI initialization FAILED.");
            return;
        }
        System.out.println("SPI initialization SUCCEEDED.");

        try {
            HttpServer server = HttpServer.create(new InetSocketAddress(USED_PORT), 0);
            server.createContext("/" + PI_URL_SUFFIX_START, new StartHandler());
            server.createContext("/" + PI_URL_SUFFIX_STOP, new StopHandler());
            server.createContext("/" + PI_URL_SUFFIX_QUIT, new QuitHandler());
            server.createContext("/" + PI_URL_SUFFIX_SETLIGHTS, new SetLightsHandler());
            server.setExecutor(null); // creates a default executor
            server.start();
        } catch (Exception exc) {
            exc.printStackTrace();
        }

        ledStrip = new LedStrip(NUMBER_OF_LEDS, 0.5F);
        ledStrip.allOff();
        ledStrip.update();
    }

    static class StartHandler implements HttpHandler {
        @Override
        public void handle(final HttpExchange t) throws IOException {
            Runnable r = new Runnable() {
                @Override
                public void run() {
                    try {
                        isInitialized = true;
                    } catch (Exception exc) {
                        exc.printStackTrace();
                    }
                }
            };
            Thread th = new Thread(r);
            th.start();

            t.sendResponseHeaders(200, 0); //We received a request, send back OK
            OutputStream os = t.getResponseBody();
            os.write(empty_arr, 0, 0);
            os.close();
        }
    }

    static class StopHandler implements HttpHandler {
        @Override
        public void handle(final HttpExchange t) throws IOException {
            Runnable r = new Runnable() {
                @Override
                public void run() {
                    try {
                        ledStrip.allOff();
                        ledStrip.update();
                        isInitialized = false;
                    } catch (Exception exc) {
                        exc.printStackTrace();
                    }
                }
            };
            Thread th = new Thread(r);
            th.start();

            t.sendResponseHeaders(200, 0); //We received a request, send back OK
            OutputStream os = t.getResponseBody();
            os.write(empty_arr, 0, 0);
            os.close();
        }
    }

    static class QuitHandler implements HttpHandler {
        @Override
        public void handle(final HttpExchange t) throws IOException {
            Runnable r = new Runnable() {
                @Override
                public void run() {
                    try {
                        ledStrip.allOff();
                        ledStrip.update();
                        isInitialized = false;
                        System.exit(0);
                    } catch (Exception exc) {
                        exc.printStackTrace();
                    }
                }
            };
            Thread th = new Thread(r);
            th.start();

            t.sendResponseHeaders(200, 0); //We received a request, send back OK
            OutputStream os = t.getResponseBody();
            os.write(empty_arr, 0, 0);
            os.close();
        }
    }

    static class SetLightsHandler implements HttpHandler {
        @Override
        public void handle(final HttpExchange t) throws IOException {
            final CountDownLatch latch = new CountDownLatch(1);
            Runnable r = new Runnable() {
                @Override
                public void run() {
                    try {

                        InputStream request = t.getRequestBody();

                        int i;
                        byte c[] = new byte[512];
                        ByteArrayOutputStream baos = new ByteArrayOutputStream();
                        while ((i = request.read(c)) != -1) {
                            baos.write(c, 0, i);
                        }
                        latch.countDown();

                        Gson g = new Gson();
                        RPI_Packet packet = g.fromJson(new String(baos.toByteArray()), RPI_Packet.class);

                        if (packet == null) {
                            System.out.println("Packet is null!");
                        } else {
                            if (isInitialized) {
                                for (int led_index = 0; led_index < NUMBER_OF_LEDS; led_index++) {
                                    if (packet.col.length > led_index) {
                                        int color = packet.col[led_index];
                                        int R = color >> 16;
                                        int G = (color >> 8) & 255;
                                        int B = color & 255;

                                        ledStrip.setLed(led_index + 1, R, G, B, 1.0f);
                                    }
                                }
                                ledStrip.update();
                            }
                        }

                    } catch (Exception exc) {
                        exc.printStackTrace();
                    }
                }
            };
            Thread th = new Thread(r);
            th.start();
            try {
                latch.await();
            } catch (InterruptedException e) {
                e.printStackTrace();
            }
            t.sendResponseHeaders(200, 0); //We received a request, send back OK
            OutputStream os = t.getResponseBody();
            os.write(empty_arr, 0, 0);
            os.close();
        }
    }

    private class RPI_Packet {
        public int[] col;
    }
}