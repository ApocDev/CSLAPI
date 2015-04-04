using System;

namespace CSLAPI.Utils
{
	public static class ArrayUtils
	{
		public static void Resize<T>(Array32<T> array, uint newSize)
		{
			array.m_size = newSize;
			Array.Resize(ref array.m_buffer, (int) newSize);
			var unusedCount = ReflectionUtils.GetField<uint>(array, "m_unusedCount");
			var unusedItems = ReflectionUtils.GetField<uint[]>(array, "m_unusedItems");

			uint[] newUnusedItems = new uint[newSize];
			Buffer.BlockCopy(unusedItems, 0, newUnusedItems, 0, 4 * unusedItems.Length);

			// Now add our own unused items
			for (uint i = (uint) unusedItems.Length; i < newSize + 1; i++)
			{
				newUnusedItems[i - 1] = i;
			}

			// Update the unusedCount to be in line with the new array size
			// This is just adding the newly sized additions.
			unusedCount += newSize - unusedCount;

			ReflectionUtils.SetField(array, "m_unusedCount", unusedCount);
			ReflectionUtils.SetField(array, "m_unusedItems", unusedItems);

			// var nextFree = ReflectionUtils.Invoke<uint>(array, "NextFreeItem");
			// var nextFree = array.NextFreeItem();
		}

		public static void Resize<T>(Array16<T> array, ushort newSize)
		{
			array.m_size = newSize;
			Array.Resize(ref array.m_buffer, newSize);

			var unusedCount = ReflectionUtils.GetField<uint>(array, "m_unusedCount");
			var unusedItems = ReflectionUtils.GetField<ushort[]>(array, "m_unusedItems");

			ushort[] newUnusedItems = new ushort[newSize];
			Buffer.BlockCopy(unusedItems, 0, newUnusedItems, 0, 4 * unusedItems.Length);

			// Now add our own unused items
			for (uint i = (uint) unusedItems.Length; i < newSize + 1; i++)
			{
				newUnusedItems[i - 1] = (ushort) i;
			}

			// Update the unusedCount to be in line with the new array size
			// This is just adding the newly sized additions.
			unusedCount += newSize - unusedCount;

			ReflectionUtils.SetField(array, "m_unusedCount", unusedCount);
			ReflectionUtils.SetField(array, "m_unusedItems", unusedItems);
		}

		public static void Resize<T>(Array8<T> array, byte newSize)
		{
			array.m_size = newSize;
			Array.Resize(ref array.m_buffer, newSize);

			var unusedCount = ReflectionUtils.GetField<uint>(array, "m_unusedCount");
			var unusedItems = ReflectionUtils.GetField<byte[]>(array, "m_unusedItems");

			byte[] newUnusedItems = new byte[newSize];
			Buffer.BlockCopy(unusedItems, 0, newUnusedItems, 0, 4 * unusedItems.Length);

			// Now add our own unused items
			for (uint i = (uint) unusedItems.Length; i < newSize + 1; i++)
			{
				newUnusedItems[i - 1] = (byte) i;
			}

			// Update the unusedCount to be in line with the new array size
			// This is just adding the newly sized additions.
			unusedCount += newSize - unusedCount;

			ReflectionUtils.SetField(array, "m_unusedCount", unusedCount);
			ReflectionUtils.SetField(array, "m_unusedItems", unusedItems);
		}
	}
}