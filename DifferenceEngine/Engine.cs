using System;
using System.Collections;

namespace DifferenceEngine
{
    public enum DiffEngineLevel
    {
        Fast,
        Medium,
        Slow
    }

	public class Engine
	{
		private IDiffList _source;
		private IDiffList _destination;
		private ArrayList _matchList;

		private DiffEngineLevel _level;
		private DiffStateList _diffStateList;

		public Engine()
		{
			_source = null;
			_destination = null;
			_matchList = null;
			_diffStateList = null;
			_level = DiffEngineLevel.Fast;
		}

		private int GetSourceMatchLength(int destIndex, int sourceIndex, int maxLength)
		{
			int matchCount;
			for (matchCount = 0; matchCount < maxLength; matchCount++)
			{
				if (_destination.GetByIndex(destIndex + matchCount).CompareTo(_source.GetByIndex(sourceIndex + matchCount)) != 0)
				{
					break;
				}
			}
			return matchCount;
		}

		private void GetLongestSourceMatch(DiffState curItem, int destIndex, int destEnd, int sourceStart, int sourceEnd)
		{

			int maxDestLength = (destEnd - destIndex) + 1,
				curLength = 0,
				curBestLength = 0,
				curBestIndex = -1,
				maxLength = 0;

			for (int sourceIndex = sourceStart; sourceIndex <= sourceEnd; sourceIndex++)
			{
				maxLength = Math.Min(maxDestLength, (sourceEnd - sourceIndex) + 1);
				if (maxLength <= curBestLength)
				{
					//No chance to find a longer one any more
					break;
				}
				curLength = GetSourceMatchLength(destIndex, sourceIndex, maxLength);
				if (curLength > curBestLength)
				{
					//This is the best match so far
					curBestIndex = sourceIndex;
					curBestLength = curLength;
				}
				//jump over the match
				sourceIndex += curBestLength;
			}

			if (curBestIndex == -1)
			{
				curItem.SetStatusToNoMatch();
			}
			else
			{
				curItem.SetStatusToMatch(curBestIndex, curBestLength);
			}

		}

		private void ProcessRange(int destStart, int destEnd, int sourceStart, int sourceEnd)
		{
			int curBestIndex = -1;
			int curBestLength = -1;
			int maxPossibleDestLength = 0;
			DiffState curItem = null;
			DiffState bestItem = null;

			for (int destIndex = destStart; destIndex <= destEnd; destIndex++)
			{
				maxPossibleDestLength = (destEnd - destIndex) + 1;
				if (maxPossibleDestLength <= curBestLength)
				{
					//we won't find a longer one even if we looked
					break;
				}
				curItem = _diffStateList.GetByIndex(destIndex);

				if (!curItem.HasValidLength(sourceStart, sourceEnd, maxPossibleDestLength))
				{
					//recalc new best length since it isn't valid or has never been done.
					GetLongestSourceMatch(curItem, destIndex, destEnd, sourceStart, sourceEnd);
				}
				if (curItem.Status == DiffStatus.Matched)
				{
					switch (_level)
					{
						case DiffEngineLevel.Fast:
							if (curItem.Length > curBestLength)
							{
								//this is longest match so far
								curBestIndex = destIndex;
								curBestLength = curItem.Length;
								bestItem = curItem;
							}
							//Jump over the match 
							destIndex += curItem.Length - 1;
							break;
						case DiffEngineLevel.Medium:
							if (curItem.Length > curBestLength)
							{
								//this is longest match so far
								curBestIndex = destIndex;
								curBestLength = curItem.Length;
								bestItem = curItem;
								//Jump over the match 
								destIndex += curItem.Length - 1;
							}
							break;
						default:
							if (curItem.Length > curBestLength)
							{
								//this is longest match so far
								curBestIndex = destIndex;
								curBestLength = curItem.Length;
								bestItem = curItem;
							}
							break;
					}
				}
			}

			if (curBestIndex < 0)
			{
				// there are no matches in this span
			}
			else
			{

				int sourceIndex = bestItem.StartIndex;
				_matchList.Add(DiffResultSpan.CreateNoChange(curBestIndex, sourceIndex, curBestLength));

				if (destStart < curBestIndex && sourceStart < sourceIndex)
				{
					// Recursive call to process lower indexes
					ProcessRange(destStart, curBestIndex - 1, sourceStart, sourceIndex - 1);
				}
				int upperDestStart = curBestIndex + curBestLength;
				int upperSourceStart = sourceIndex + curBestLength;

				if (destEnd > upperDestStart && sourceEnd > upperSourceStart)
				{
					// Recursive call to process upper indexes
					ProcessRange(upperDestStart, destEnd, upperSourceStart, sourceEnd);

				}
			}
		}

		public double ProcessDiff(IDiffList source, IDiffList destination)
		{
			DateTime dt = DateTime.Now;
			_source = source;
			_destination = destination;
			_matchList = new ArrayList();

			int dcount = _destination.Count();
			int scount = _source.Count();


			if ((dcount > 0) && (scount > 0))
			{
				_diffStateList = new DiffStateList(dcount);
				ProcessRange(0, dcount - 1, 0, scount - 1);
			}

			TimeSpan ts = DateTime.Now - dt;
			return ts.TotalSeconds;
		}

		private bool AddChanges(ArrayList report, int curDest, int nextDest, int curSource, int nextSource)
		{
			bool IsDifferent = false;
			int diffDest = nextDest - curDest;
			int diffSource = nextSource - curSource;
			int minDiff = 0;
			if (diffDest > 0)
			{
				if (diffSource > 0)
				{
					minDiff = Math.Min(diffDest, diffSource);
					report.Add(DiffResultSpan.CreateReplace(curDest, curSource, minDiff));
					if (diffDest > diffSource)
					{
						curDest += minDiff;
						report.Add(DiffResultSpan.CreateAddDestination(curDest, diffDest - diffSource));
					}
					else
					{
						if (diffSource > diffDest)
						{
							curSource += minDiff;
							report.Add(DiffResultSpan.CreateDeleteSource(curSource, diffSource - diffDest));
						}
					}
				}
				else
				{
					report.Add(DiffResultSpan.CreateAddDestination(curDest, diffDest));
				}
				IsDifferent = true;
			}
			else
			{
				if (diffSource > 0)
				{
					report.Add(DiffResultSpan.CreateDeleteSource(curSource, diffSource));
					IsDifferent = true;
				}
			}
			return IsDifferent;
		}

		public ArrayList DiffReport()
		{
			ArrayList retval = new ArrayList();
			int dcount = _destination.Count();
			int scount = _source.Count();

			//Deal with the special case of empty files
			if (dcount == 0)
			{
				if (scount > 0)
				{
					retval.Add(DiffResultSpan.CreateDeleteSource(0, scount));
				}
				return retval;
			}
			else
			{
				if (scount == 0)
				{
					retval.Add(DiffResultSpan.CreateAddDestination(0, dcount));
					return retval;
				}
			}


			_matchList.Sort();
			int curDest = 0;
			int curSource = 0;
			DiffResultSpan last = null;

			//Process each match record
			foreach (DiffResultSpan drs in _matchList)
			{
				if ((!AddChanges(retval, curDest, drs.DestIndex, curSource, drs.SourceIndex)) &&
					(last != null))
				{
					last.AddLength(drs.Length);
				}
				else
				{
					retval.Add(drs);
				}
				curDest = drs.DestIndex + drs.Length;
				curSource = drs.SourceIndex + drs.Length;
				last = drs;
			}

			//Process any tail end data
			AddChanges(retval, curDest, dcount, curSource, scount);

			return retval;
		}

	}
}
