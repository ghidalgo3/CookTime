using System;

namespace babe_algorithms.Models
{
    public class Image
    {
        public Guid Id { get; set; }
        public byte[] Data { get; set; }
    }
}