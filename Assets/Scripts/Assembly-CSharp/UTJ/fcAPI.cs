using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UTJ
{
	public static class fcAPI
	{
		public enum fcPixelFormat
		{
			Unknown = 0,
			ChannelMask = 15,
			TypeMask = 240,
			Type_f16 = 16,
			Type_f32 = 32,
			Type_u8 = 48,
			Type_i16 = 64,
			Type_i32 = 80,
			Rf16 = 17,
			RGf16 = 18,
			RGBf16 = 19,
			RGBAf16 = 20,
			Rf32 = 33,
			RGf32 = 34,
			RGBf32 = 35,
			RGBAf32 = 36,
			Ru8 = 49,
			RGu8 = 50,
			RGBu8 = 51,
			RGBAu8 = 52,
			Ri16 = 65,
			RGi16 = 66,
			RGBi16 = 67,
			RGBAi16 = 68,
			Ri32 = 81,
			RGi32 = 82,
			RGBi32 = 83,
			RGBAi32 = 84
		}

		public enum fcDownloadState
		{
			Idle = 0,
			Completed = 1,
			Error = 2,
			InProgress = 3
		}

		public struct fcStream
		{
			public IntPtr ptr;
		}

		public struct fcPngConfig
		{
			public int max_active_tasks;

			public static fcPngConfig default_value
			{
				get
				{
					return new fcPngConfig
					{
						max_active_tasks = 0
					};
				}
			}
		}

		public struct fcPNGContext
		{
			public IntPtr ptr;
		}

		public struct fcExrConfig
		{
			public int max_active_tasks;

			public static fcExrConfig default_value
			{
				get
				{
					return new fcExrConfig
					{
						max_active_tasks = 0
					};
				}
			}
		}

		public struct fcEXRContext
		{
			public IntPtr ptr;
		}

		public struct fcGifConfig
		{
			public int width;

			public int height;

			public int num_colors;

			public int max_active_tasks;

			public static fcGifConfig default_value
			{
				get
				{
					return new fcGifConfig
					{
						width = 320,
						height = 240,
						num_colors = 256,
						max_active_tasks = 0
					};
				}
			}
		}

		public struct fcGIFContext
		{
			public IntPtr ptr;
		}

		public struct fcMP4Config
		{
			public Bool video;

			public Bool audio;

			public Bool video_use_hardware_encoder_if_possible;

			public int video_width;

			public int video_height;

			public int video_bitrate;

			public int video_max_framerate;

			public int video_max_buffers;

			public float audio_scale;

			public int audio_sampling_rate;

			public int audio_num_channels;

			public int audio_bitrate;

			public static fcMP4Config default_value
			{
				get
				{
					return new fcMP4Config
					{
						video = true,
						audio = false,
						video_use_hardware_encoder_if_possible = true,
						video_width = 320,
						video_height = 240,
						video_bitrate = 256000,
						video_max_framerate = 30,
						video_max_buffers = 8,
						audio_scale = 32767f,
						audio_sampling_rate = 48000,
						audio_num_channels = 2,
						audio_bitrate = 64000
					};
				}
			}
		}

		public struct fcMP4Context
		{
			public IntPtr ptr;
		}

		[DllImport("FrameCapturer")]
		public static extern void fcSetModulePath(string path);

		[DllImport("FrameCapturer")]
		public static extern double fcGetTime();

		[DllImport("FrameCapturer")]
		public static extern fcStream fcCreateFileStream(string path);

		[DllImport("FrameCapturer")]
		public static extern fcStream fcCreateMemoryStream();

		[DllImport("FrameCapturer")]
		public static extern void fcDestroyStream(fcStream s);

		[DllImport("FrameCapturer")]
		public static extern ulong fcStreamGetWrittenSize(fcStream s);

		[DllImport("FrameCapturer")]
		public static extern void fcGuardBegin();

		[DllImport("FrameCapturer")]
		public static extern void fcGuardEnd();

		[DllImport("FrameCapturer")]
		public static extern void fcEraseDeferredCall(int id);

		[DllImport("FrameCapturer")]
		public static extern IntPtr fcGetRenderEventFunc();

		public static void fcGuard(Action body)
		{
			fcGuardBegin();
			body();
			fcGuardEnd();
		}

		public static fcPixelFormat fcGetPixelFormat(RenderTextureFormat v)
		{
			switch (v)
			{
			case RenderTextureFormat.ARGB32:
				return fcPixelFormat.RGBAu8;
			case RenderTextureFormat.ARGBHalf:
				return fcPixelFormat.RGBAf16;
			case RenderTextureFormat.RGHalf:
				return fcPixelFormat.RGf16;
			case RenderTextureFormat.RHalf:
				return fcPixelFormat.Rf16;
			case RenderTextureFormat.ARGBFloat:
				return fcPixelFormat.RGBAf32;
			case RenderTextureFormat.RGFloat:
				return fcPixelFormat.RGf32;
			case RenderTextureFormat.RFloat:
				return fcPixelFormat.Rf32;
			case RenderTextureFormat.ARGBInt:
				return fcPixelFormat.RGBAi32;
			case RenderTextureFormat.RGInt:
				return fcPixelFormat.RGi32;
			case RenderTextureFormat.RInt:
				return fcPixelFormat.Ri32;
			default:
				return fcPixelFormat.Unknown;
			}
		}

		public static fcPixelFormat fcGetPixelFormat(TextureFormat v)
		{
			switch (v)
			{
			case TextureFormat.Alpha8:
				return fcPixelFormat.Ru8;
			case TextureFormat.RGB24:
				return fcPixelFormat.RGBu8;
			case TextureFormat.RGBA32:
				return fcPixelFormat.RGBAu8;
			case TextureFormat.ARGB32:
				return fcPixelFormat.RGBAu8;
			case TextureFormat.RGBAHalf:
				return fcPixelFormat.RGBAf16;
			case TextureFormat.RGHalf:
				return fcPixelFormat.RGf16;
			case TextureFormat.RHalf:
				return fcPixelFormat.Rf16;
			case TextureFormat.RGBAFloat:
				return fcPixelFormat.RGBAf32;
			case TextureFormat.RGFloat:
				return fcPixelFormat.RGf32;
			case TextureFormat.RFloat:
				return fcPixelFormat.Rf32;
			default:
				return fcPixelFormat.Unknown;
			}
		}

		public static int fcGetNumAudioChannels()
		{
			switch (AudioSettings.speakerMode)
			{
			case AudioSpeakerMode.Mono:
				return 1;
			case AudioSpeakerMode.Stereo:
				return 2;
			case AudioSpeakerMode.Quad:
				return 4;
			case AudioSpeakerMode.Surround:
				return 5;
			case AudioSpeakerMode.Mode5point1:
				return 6;
			case AudioSpeakerMode.Mode7point1:
				return 8;
			case AudioSpeakerMode.Prologic:
				return 6;
			default:
				return 0;
			}
		}

		[DllImport("FrameCapturer")]
		public static extern fcPNGContext fcPngCreateContext(ref fcPngConfig conf);

		[DllImport("FrameCapturer")]
		public static extern void fcPngDestroyContext(fcPNGContext ctx);

		[DllImport("FrameCapturer")]
		private static extern int fcPngExportTextureDeferred(fcPNGContext ctx, string path, IntPtr tex, int width, int height, fcPixelFormat f, Bool flipY, int id);

		public static int fcPngExportTexture(fcPNGContext ctx, string path, RenderTexture tex, int pos)
		{
			return fcPngExportTextureDeferred(ctx, path, tex.GetNativeTexturePtr(), tex.width, tex.height, fcGetPixelFormat(tex.format), false, pos);
		}

		[DllImport("FrameCapturer")]
		public static extern fcEXRContext fcExrCreateContext(ref fcExrConfig conf);

		[DllImport("FrameCapturer")]
		public static extern void fcExrDestroyContext(fcEXRContext ctx);

		[DllImport("FrameCapturer")]
		private static extern int fcExrBeginFrameDeferred(fcEXRContext ctx, string path, int width, int height, int id);

		[DllImport("FrameCapturer")]
		private static extern int fcExrAddLayerTextureDeferred(fcEXRContext ctx, IntPtr tex, fcPixelFormat f, int ch, string name, Bool flipY, int id);

		[DllImport("FrameCapturer")]
		private static extern int fcExrEndFrameDeferred(fcEXRContext ctx, int id);

		public static int fcExrBeginFrame(fcEXRContext ctx, string path, int width, int height, int id)
		{
			return fcExrBeginFrameDeferred(ctx, path, width, height, id);
		}

		public static int fcExrEndFrame(fcEXRContext ctx, int id)
		{
			return fcExrEndFrameDeferred(ctx, id);
		}

		public static int fcExrAddLayerTexture(fcEXRContext ctx, RenderTexture tex, int ch, string name, int id)
		{
			return fcExrAddLayerTextureDeferred(ctx, tex.GetNativeTexturePtr(), fcGetPixelFormat(tex.format), ch, name, false, id);
		}

		[DllImport("FrameCapturer")]
		public static extern fcGIFContext fcGifCreateContext(ref fcGifConfig conf);

		[DllImport("FrameCapturer")]
		public static extern void fcGifDestroyContext(fcGIFContext ctx);

		[DllImport("FrameCapturer")]
		private static extern int fcGifAddFrameTextureDeferred(fcGIFContext ctx, IntPtr tex, fcPixelFormat fmt, Bool keyframe, double timestamp, int id);

		[DllImport("FrameCapturer")]
		public static extern Bool fcGifWrite(fcGIFContext ctx, fcStream stream, int begin_frame = 0, int end_frame = -1);

		[DllImport("FrameCapturer")]
		public static extern void fcGifClearFrame(fcGIFContext ctx);

		[DllImport("FrameCapturer")]
		public static extern int fcGifGetFrameCount(fcGIFContext ctx);

		[DllImport("FrameCapturer")]
		public static extern void fcGifGetFrameData(fcGIFContext ctx, IntPtr tex, int frame);

		[DllImport("FrameCapturer")]
		public static extern int fcGifGetExpectedDataSize(fcGIFContext ctx, int begin_frame, int end_frame);

		[DllImport("FrameCapturer")]
		public static extern void fcGifEraseFrame(fcGIFContext ctx, int begin_frame, int end_frame);

		public static int fcGifAddFrameTexture(fcGIFContext ctx, RenderTexture tex, bool keyframe, double timestamp, int id)
		{
			return fcGifAddFrameTextureDeferred(ctx, tex.GetNativeTexturePtr(), fcGetPixelFormat(tex.format), keyframe, timestamp, id);
		}

		public static Bool fcGifWriteFile(fcGIFContext ctx, string path, int begin_frame = 0, int end_frame = -1)
		{
			fcStream fcStream = fcCreateFileStream(path);
			Bool result = fcGifWrite(ctx, fcStream, begin_frame, end_frame);
			fcDestroyStream(fcStream);
			return result;
		}

		[DllImport("FrameCapturer")]
		public static extern void fcMP4SetFAACPackagePath(string path);

		[DllImport("FrameCapturer")]
		public static extern void fcMP4SetFAACUnzipPath(string path);

		[DllImport("FrameCapturer")]
		public static extern Bool fcMP4DownloadCodecBegin();

		[DllImport("FrameCapturer")]
		public static extern fcDownloadState fcMP4DownloadCodecGetState();

		[DllImport("FrameCapturer")]
		public static extern fcMP4Context fcMP4CreateContext(ref fcMP4Config conf);

		[DllImport("FrameCapturer")]
		public static extern void fcMP4DestroyContext(fcMP4Context ctx);

		[DllImport("FrameCapturer")]
		public static extern void fcMP4AddOutputStream(fcMP4Context ctx, fcStream s);

		[DllImport("FrameCapturer")]
		private static extern IntPtr fcMP4GetAudioEncoderInfo(fcMP4Context ctx);

		[DllImport("FrameCapturer")]
		private static extern IntPtr fcMP4GetVideoEncoderInfo(fcMP4Context ctx);

		[DllImport("FrameCapturer")]
		private static extern int fcMP4AddVideoFrameTextureDeferred(fcMP4Context ctx, IntPtr tex, fcPixelFormat fmt, double time, int id);

		[DllImport("FrameCapturer")]
		public static extern Bool fcMP4AddAudioFrame(fcMP4Context ctx, float[] samples, int num_samples, double time = -1.0);

		public static string fcMP4GetAudioEncoderInfoS(fcMP4Context ctx)
		{
			return Marshal.PtrToStringAnsi(fcMP4GetAudioEncoderInfo(ctx));
		}

		public static string fcMP4GetVideoEncoderInfoS(fcMP4Context ctx)
		{
			return Marshal.PtrToStringAnsi(fcMP4GetVideoEncoderInfo(ctx));
		}

		public static int fcMP4AddVideoFrameTexture(fcMP4Context ctx, RenderTexture tex, double time, int id)
		{
			return fcMP4AddVideoFrameTextureDeferred(ctx, tex.GetNativeTexturePtr(), fcGetPixelFormat(tex.format), time, id);
		}
	}
}
